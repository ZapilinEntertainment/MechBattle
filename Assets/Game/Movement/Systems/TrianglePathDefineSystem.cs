using Scellecs.Morpeh;
using Unity.IL2CPP.CompilerServices;
using Unity.Mathematics;
using ZE.MechBattle.Navigation;

namespace ZE.MechBattle.Ecs 
{
    [Il2CppSetOption(Option.NullChecks, false)]
    [Il2CppSetOption(Option.ArrayBoundsChecks, false)]
    [Il2CppSetOption(Option.DivideByZeroChecks, false)]
    public sealed class TrianglePathDefineSystem : ISystem 
    {
        public World World { get; set;}
        private Filter _regularHexPathUsers;
        private Filter _singleTransitionHexPathUsers;
        private Filter _noHexPathUsers;

        private Stash<HexPathComponent> _regularPaths;
        private Stash<FlowTrianglePathComponent> _flowPaths;
        private Stash<TriangularPosComponent> _triangularPos;
        private Stash<MoveTargetComponent> _moveTargets;
        private Stash<ClearHexPathTag> _invalidHexPaths;
        private Stash<CalculatingTrianglePathComponent> _endpoints;
        private Stash<HexCoordComponent> _hexCoord;
        private Stash<TrianglePathDefinedTag> _trianglePathDefined;
        private Stash<TransitionHexPathComponent> _transitionHexPathComponents;
        private Stash<HexPathFailPointComponent> _failPoints;

        private readonly HexPathsLRUBuffer _hexPathsList;
        private readonly INavigationMap _map;
       
        public TrianglePathDefineSystem(HexPathsLRUBuffer hexPathsList, INavigationMap map) 
        {
            _hexPathsList = hexPathsList;
            _map = map;
        }

        public void OnAwake() 
        {

            _regularHexPathUsers = World.Filter
                .With<HexPathDefinedTag>()
                .With<HexPathComponent>()
                .Without<HexPathSelectRequestComponent>()
                .Without<TrianglePathDefinedTag>()                
                .Build();

            _singleTransitionHexPathUsers = World.Filter
                .With<HexPathDefinedTag>()
                .With<TransitionHexPathComponent>()
                .Without<TrianglePathDefinedTag>()                
                .Build();

            _noHexPathUsers = World.Filter
                .With<EmptyHexPathTag>()
                .Without<TrianglePathDefinedTag>()
                .Build();

            _regularPaths = World.GetStash<HexPathComponent>();
            _flowPaths = World.GetStash<FlowTrianglePathComponent>();
            _triangularPos = World.GetStash<TriangularPosComponent>();
            _moveTargets = World.GetStash<MoveTargetComponent>();
            _invalidHexPaths = World.GetStash<ClearHexPathTag>();
            _endpoints = World.GetStash<CalculatingTrianglePathComponent>();
            _hexCoord = World.GetStash<HexCoordComponent>();
            _failPoints = World.GetStash<HexPathFailPointComponent>();

            _trianglePathDefined = World.GetStash<TrianglePathDefinedTag>();
            _transitionHexPathComponents = World.GetStash<TransitionHexPathComponent>();
        }

        private struct TrianglePathStrategy
        {
            public bool BuildTrianglePath;
            public HexEdge TargetEdge;
            public IntTriangularPos TargetPos;
        }

        public void OnUpdate(float deltaTime) 
        {
            // move inside hex
            foreach (var entity in _noHexPathUsers)
            {
                UnityEngine.Debug.Log("no path");
                RequestTrianglePathCalculationToFinalTarget(entity);
            }

            // transite from one hex into another
            foreach (var entity in _singleTransitionHexPathUsers)
            {
                var hexCoord = _hexCoord.Get(entity).Value;
                var transitionComponent = _transitionHexPathComponents.Get(entity);
                UnityEngine.Debug.Log("single transition");
                if (math.all(hexCoord == transitionComponent.TargetHex))
                {
                    // already in target hex
                    RequestTrianglePathCalculationToFinalTarget(entity);
                }
                else
                {
                    // still in start hex
                   SetupFlowMapMovement(entity, transitionComponent.TransitionEdge, transitionComponent.TargetHex);
                }
            }

            // not-neighboured hex path
            foreach (var entity in _regularHexPathUsers)
            {
                var hexPathComponent = _regularPaths.Get(entity);
                var stepIndex = hexPathComponent.StepIndex;
                UnityEngine.Debug.Log($"hex step index: {stepIndex} / {hexPathComponent.StepsCount}");

                if (stepIndex == hexPathComponent.StepsCount)
                {
                    // last hex node -> target
                    UnityEngine.Debug.Log($"moving to final target");
                    RequestTrianglePathCalculationToFinalTarget(entity);
                }
                else
                {
                    // start pos -> first node
                    // or node X -> node X+1
                    if (!_hexPathsList.TryGetPath(hexPathComponent.PathId, out var path) 
                        || !path.TryGetNode(stepIndex, out var currentNode)
                        || !IfPathIsCorrect(entity, currentNode.Edge, stepIndex))
                    {
                        _invalidHexPaths.Set(entity);
                        continue;
                    }
                    

                    var currentHexCoord = _hexCoord.Get(entity).Value;
                    var nextHexCoord = currentHexCoord + currentNode.Edge.ToHexOffsetVector();

                    SetupFlowMapMovement(entity, currentNode.Edge, nextHexCoord);
                    UnityEngine.Debug.Log($"use flow map: {currentNode.Edge} of {currentNode.HexCoord}");
                    // note: step index will be increased by HexPathProgressionSystem when reach target
                }
            }
        }

        public void Dispose() { }

        private void RequestTrianglePathCalculationToFinalTarget(Entity entity)
        {
            var start = _triangularPos.Get(entity).Value;
            var target = _moveTargets.Get(entity).TriangularPos;
            _endpoints.Set(entity, new(start, target));
            _trianglePathDefined.Add(entity);
        }

        private void SetupFlowMapMovement(Entity entity, HexEdge exitEdge, int2 nextHexCoord)
        {
            //UnityEngine.Debug.Log($"flow movement to {nextHexCoord}:{exitEdge}");
            _flowPaths.Set(entity, new(exitEdge, nextHexCoord));
            _trianglePathDefined.Add(entity);
        }

        private bool IfPathIsCorrect(Entity entity, HexEdge exitEdge, int stepIndex)
        {
            // check if next node is really accessible
            var tripos = _triangularPos.Get(entity).Value;
            var cellEdgesAccessMask = _map.GetFlowData(tripos).GetCombinedEdgeAccessMask();
            if (!cellEdgesAccessMask.IsEdgePresented(exitEdge))
            {
                UnityEngine.Debug.Log($"hex path {_regularPaths.Get(entity).PathId} was incorrect: edge is not accessible");
                _failPoints.Set(entity, new(stepIndex));
                return false;
            }

            return true;
        }
    }
}