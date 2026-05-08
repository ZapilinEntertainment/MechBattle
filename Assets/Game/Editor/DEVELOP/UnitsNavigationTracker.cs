using System;
using System.Buffers;
using System.Collections.Generic;
using UnityEngine;
using Unity.Mathematics;
using VContainer;
using Scellecs.Morpeh;
using ZE.MechBattle;
using ZE.MechBattle.Ecs;
using ZE.MechBattle.Navigation;
using TriInspector;

namespace ZE.MechBattle.Develop
{
    public class UnitsNavigationTracker : MonoBehaviour
    {
        [Serializable]
        public enum HexPathStatus : byte
        {
            Undefined,
            HexPathNotNeeded,
            SingleTransition,
            RegularPath,
            RegularPathNotFound,
        }

        [Serializable]
        public enum TrianglePathStatus : byte
        {
            Undefined,
            DefinedButNoComponent,
            UsingFlowMap,
            UsingRegularPath,
            RegularPathNotFound,
        }

        [Serializable]
        public struct UnitNavigationData
        {
            public Entity Entity;
            [Space]
            public bool MoveTargetSet;
            public float3 MoveTarget;
            [Space]
            public int HexPathId;
            public HexPathStatus HexPathStatus;
            public SerializedHexPathNode[] HexPathNodes;
            public bool HexPathClearRequired;
            public bool IsHexPathCalculating;
            public bool IsHexPathDefined;
            [Space]
            public int TrianglePathId;
            public TrianglePathStatus TrianglePathStatus;
            public IntTriangularPos[] TriangularPositions;
            public HexEdge ExitEdge;
            public bool TrianglePathDefined;
            public bool TrianglePathClearRequired;
            public bool IsTrianglePathCalculating;
            public bool IsTrianglePathCompleted;
            [Space]
            public bool WaypointSet;
            public IntTriangularPos WaypointMoveTarget;
        }

        [Serializable]
        public struct SerializedHexPathNode
        {
            [ReadOnly] public int2 HexCoord;
            [ReadOnly] public HexEdge Edge;

            public SerializedHexPathNode(HexPathNodeKey node)
            {
                HexCoord = node.HexCoord;
                Edge = node.Edge;
            }

            public SerializedHexPathNode(int2 hexCoord, HexEdge edge)
            {
                HexCoord = hexCoord;
                Edge = edge;
            }

            public static SerializedHexPathNode[] ConvertPoints(HexPathNodeKey[] originalNodes)
            {
                var array = new SerializedHexPathNode[originalNodes.Length];
                for (var i = 0; i < originalNodes.Length; i++)
                {
                    array[i] = new(originalNodes[i]);
                }
                return array;
            }
        }

        [ReadOnly, SerializeField] private List<UnitNavigationData> _data = new();
        [SerializeField] private bool _updateEveryTick = false;
        private List<IntTriangularPos> _occupiedTris = new();

        private World _world;
        private Filter _filter;
        private NavigationHexPathsList _hexPathsList;
        private NavigationTrianglePathsBuffer _trianglePathsBuffer;
        private INavigationMap _map;

        private Stash<MoveTargetComponent> _moveTargets;
        private Stash<WaypointMoveTarget> _waypoints;

        private Stash<RegularHexPathComponent> _regularHexPaths;
        private Stash<TransitionHexPathComponent> _transitionHexPaths;
        private Stash<HexPathDefinedTag> _hexPathDefinedTags;
        private Stash<ClearHexPathTag> _clearHexPathTags;
        private Stash<CalculatingHexPathComponent> _calculatingHexPathComponents;

        private Stash<TrianglePathDefinedTag> _trianglePathDefined;
        private Stash<RegularTrianglePathComponent> _regularTrianglePaths;
        private Stash<FlowTrianglePathComponent> _flowTrianglePaths;       
        private Stash<ClearTrianglePathTag> _clearTriangleTags;
        private Stash<CalculatingTrianglePathComponent> _calculatingTrianglePathComponents;
        private Stash<CompletedTrianglePathTag> _completedTrianglePaths;

        private Stash<TriangularPosComponent> _triangularPosComponents;
        

        [Inject]
        public void Inject(
            World world, 
            NavigationHexPathsList hexPathsList, 
            NavigationTrianglePathsBuffer trianglePathsBuffer,
            INavigationMap map)
        {
            _world = world;
            _hexPathsList = hexPathsList;
            _trianglePathsBuffer = trianglePathsBuffer;
            _map = map;

            _filter = _world.Filter.With<NavigationAgentComponent>().Build();
            _moveTargets = _world.GetStash<MoveTargetComponent>();
            _waypoints = _world.GetStash<WaypointMoveTarget>();

            _hexPathDefinedTags = world.GetStash<HexPathDefinedTag>();
            _regularHexPaths = _world.GetStash<RegularHexPathComponent>();
            _transitionHexPaths = world.GetStash<TransitionHexPathComponent>();            
            _clearHexPathTags = _world.GetStash<ClearHexPathTag>();
            _calculatingHexPathComponents = _world.GetStash<CalculatingHexPathComponent>();

            _trianglePathDefined = world.GetStash<TrianglePathDefinedTag>();
            _regularTrianglePaths = world.GetStash<RegularTrianglePathComponent>();
            _flowTrianglePaths = world.GetStash<FlowTrianglePathComponent>();
            _clearTriangleTags = world.GetStash<ClearTrianglePathTag>();
            _calculatingTrianglePathComponents = world.GetStash<CalculatingTrianglePathComponent>();   
            _completedTrianglePaths = world.GetStash<CompletedTrianglePathTag>();

            _triangularPosComponents = world.GetStash<TriangularPosComponent>();
        }

        private void Update()
        {
            if (_updateEveryTick)
                UpdateData();
        }

        [Button("Update data")]
        private void UpdateData()
        {
            if (_world == null)
                return;

            _data.Clear();
            _occupiedTris.Clear();
            foreach (var entity in _filter)
            {
                var entityData = new UnitNavigationData();
                entityData.Entity = entity;

                var moveTargetComponent = _moveTargets.Get(entity, out var movementTargetSet);
                entityData.MoveTargetSet = movementTargetSet;
                entityData.MoveTarget = movementTargetSet ? moveTargetComponent.WorldPos : float3.zero;

                UpdateHexPathData(entity, ref entityData);
                UpdateTrianglePathData(entity, ref entityData);

                var waypointComponent = _waypoints.Get(entity, out var waypointSet);
                entityData.WaypointSet = waypointSet;
                entityData.WaypointMoveTarget = waypointSet ? waypointComponent.TriangularPos : default;

                _data.Add(entityData);

                _occupiedTris.Add(_triangularPosComponents.Get(entity).Value);
            }
        }


        private void UpdateHexPathData(Entity entity, ref UnitNavigationData entityData)
        {
            var hexPathDefined = _hexPathDefinedTags.Has(entity);
            entityData.IsHexPathDefined = hexPathDefined;

            if (hexPathDefined)
            {
                var regularPathComponent = _regularHexPaths.Get(entity, out var haveRegularHexPath);
                var transitionPathComponent = _transitionHexPaths.Get(entity, out var haveTransitionHexPath);

                if (haveRegularHexPath)
                {
                    entityData.HexPathId = regularPathComponent.PathId;
                    if (_hexPathsList.TryGetPath(regularPathComponent.PathId, out var path))
                    {
                        entityData.HexPathNodes = SerializedHexPathNode.ConvertPoints(path.Points);
                        entityData.HexPathStatus = HexPathStatus.RegularPath;
                    }
                    else
                    {
                        entityData.HexPathStatus = HexPathStatus.RegularPathNotFound;
                    }
                }
                else
                {
                    if (haveTransitionHexPath)
                    {
                        entityData.HexPathStatus = HexPathStatus.SingleTransition;
                        entityData.HexPathNodes = new SerializedHexPathNode[1] { new(transitionPathComponent.TargetHex, transitionPathComponent.TransitionEdge) };
                    }
                    else
                    {
                        entityData.HexPathStatus = HexPathStatus.HexPathNotNeeded;
                    }
                }
            }
            else
            {
                entityData.HexPathId = -1;
                entityData.HexPathNodes = default;
                entityData.HexPathStatus = HexPathStatus.HexPathNotNeeded;
            }

            entityData.HexPathClearRequired = _clearHexPathTags.Has(entity);
            entityData.IsHexPathCalculating = _calculatingHexPathComponents.Has(entity);            
        }

        private void UpdateTrianglePathData(Entity entity, ref UnitNavigationData entityData)
        {
            var trianglePathDefined = _trianglePathDefined.Has(entity);
            entityData.TrianglePathDefined = trianglePathDefined;

            if (trianglePathDefined)
            {
                var regularPathComponent = _regularTrianglePaths.Get(entity, out var regularTrianglePathPresented);
                var flowPathComponent = _flowTrianglePaths.Get(entity, out var flowPathPresented); 

                if (regularTrianglePathPresented)
                {
                    if (_trianglePathsBuffer.TryGetPath(regularPathComponent.PathId, out var pathData))
                    {
                        entityData.TrianglePathStatus = TrianglePathStatus.UsingRegularPath;
                        entityData.TriangularPositions = pathData.Points;
                    }
                    else
                    {
                        entityData.TrianglePathStatus = TrianglePathStatus.RegularPathNotFound;
                    }
                }
                else
                {
                    if (flowPathPresented)
                    {
                        entityData.ExitEdge = flowPathComponent.ExitEdge;
                        entityData.TrianglePathStatus = TrianglePathStatus.UsingFlowMap;
                    }
                    else
                    {
                        entityData.TrianglePathStatus = TrianglePathStatus.DefinedButNoComponent;
                    }
                }
            }
            else
            {
                entityData.TrianglePathStatus = TrianglePathStatus.Undefined;
            }

            entityData.TrianglePathClearRequired = _clearTriangleTags.Has(entity);
            entityData.IsTrianglePathCalculating = _calculatingTrianglePathComponents.Has(entity);
            entityData.IsTrianglePathCompleted = _completedTrianglePaths.Has(entity);
        }

        #if UNITY_EDITOR
        private void OnDrawGizmos()
        {
            if (!Application.isPlaying)
                return;

            var height = _map.TriangleHeight;
            foreach (var tripos in _occupiedTris)
            {
                var vertices = GetTriangleVerticesCommand.Execute(tripos, height, 0f);
                Gizmos.DrawLine(vertices.PinnaclePos, vertices.LeftBasisPos);
                Gizmos.DrawLine(vertices.LeftBasisPos, vertices.RightBasisPos);
                Gizmos.DrawLine(vertices.RightBasisPos, vertices.PinnaclePos);
            }
        }
        #endif
    }
}
 