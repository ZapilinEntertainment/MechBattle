using System;
using System.Collections.Generic;
using UnityEngine;
using Unity.Mathematics;
using Unity.Collections;
using ZE.Utils;
using ZE.MechBattle.Navigation.PortalPathCalculation;

namespace ZE.MechBattle.Navigation
{
    public struct PortalConstructionProcessInput
    {
        public HexPathSearchRequest Request;
        public int ReservedPathId;
    }

    public readonly struct PortalConstructionProcessOutput : IDisposable
    {
        public readonly bool IsValid;
        public readonly int PathId;       
        public readonly PathCalculationResult<PortalPathDestinationKey, int> Result;
        public IDisposable DisposableResource => _resultingPathSourceArray;
        private readonly NativeArray<int> _resultingPathSourceArray;

        public PortalConstructionProcessOutput(
            int pathId,
            NativeArray<int> resultingPathSourceArray, 
            PathCalculationResult<PortalPathDestinationKey, int> result)
        {
            IsValid = true;
            PathId = pathId;
            _resultingPathSourceArray = resultingPathSourceArray;
            Result = result;
        }

        public void Dispose()
        {
            if (!IsValid)
                return;

            _resultingPathSourceArray.Dispose();
        }
    }

    public class PortalsPathConstructionProcess : AsyncProcessBase<PortalConstructionProcessInput>, IPortalsPathCalculationObject
    {
        public PortalConstructionProcessOutput Output { get; private set; }
        protected override bool IsDisposeAvailable => _isDisposeAvailable;

        #region IPortalsPathCalculationObject

        StartPortalsList IPortalsPathCalculationObject.StartPortals => _startPortals;
        EndPortalsList IPortalsPathCalculationObject.EndPortals => _endPortals;
        Dictionary<int, PortalNode> IPortalsPathCalculationObject.Nodes => _nodes;
        HashSet<int> IPortalsPathCalculationObject.ActiveNodeIds => _activeNodeIds;
        IPortalsHandler IPortalsPathCalculationObject.PortalsHandler => _portalLogic;
        IHexPortalsCoordinator IPortalsPathCalculationObject.PortalsCoordinator => _portalsCoordinator;
        NativeList<int> IPortalsPathCalculationObject.ResultingPath { get => _resultingPath; set => _resultingPath = value; }

        #endregion

        private readonly INavigationMap _map;
        private readonly IHexPortalsCoordinator _portalsCoordinator;
        private readonly IPortalsHandler _portalLogic;

        private readonly CalculatePointDistancesProcess _calculateDistancesProcess;
        private readonly StartPortalsList _startPortals = new();
        private readonly EndPortalsList _endPortals = new();
        private readonly List<HexExitOption> _hexPortalsList = new();

        private readonly Dictionary<int, PortalNode> _nodes = new();
        private readonly HashSet<int> _activeNodeIds = new();

        private bool _isDisposeAvailable = true;
        private NativeList<int> _resultingPath;

        public PortalsPathConstructionProcess(
            Allocator allocator, 
            INavigationMap map,
            IHexPortalsCoordinator portalsCoordinator,
            IPortalsHandler portalLogic)
        {
            _map = map;
            _portalsCoordinator = portalsCoordinator;
            _calculateDistancesProcess = new(allocator, _map);
            _portalLogic = portalLogic;

            _resultingPath = new(allocator);
        }

        protected override void DisposeResources()
        {
#if UNITY_EDITOR
            if (EditorPlaymodeLifetimeObject.IsQuitting)
            {
                try
                {
                    FinalDispose();
                }
                catch
                {
                    // ignore it
                }
                finally
                {
                }
            }
                return;
#else
            FinalDispose();
#endif
        }

        private void FinalDispose()
        {
            _calculateDistancesProcess.Dispose();
            _resultingPath.Dispose();
        }

        protected override async Awaitable ExecuteAsync(PortalConstructionProcessInput input)
        {
            var request = input.Request;

#if ZE_NAVIGATION_DEBUG
            if (NavigationLogger.Settings.HasFlag(NavigationLogEvents.HexPathCalculationStart))
                UnityEngine.Debug.Log($"hex path calculation started: {input.ReservedPathId}");
#endif

            await PreparePortalOptions(request.StartHexZoneIndex, request.StartHexCoord, request.EndHexCoord, request.StartTripos, _startPortals);
            await PreparePortalOptions(request.EndHexZoneIndex, request.EndHexCoord, request.StartHexCoord, request.EndTripos, _endPortals);

            if (_startPortals.Count == 0)
            {
                UnityEngine.Debug.LogError($"start hex{input.Request.StartHexCoord} zone {input.Request.StartHexZoneIndex}  has no portals");
                return;
            }

            if (_endPortals.Count == 0)
            {
                UnityEngine.Debug.LogError($"end hex {input.Request.EndHexCoord} zone {input.Request.EndHexZoneIndex}  has no portals");
                return;
            }
            //  sort start portals from closest to farthest
            _startPortals.Sort((optionA, optionB) => optionA.MinDist.CompareTo(optionB.MinDist));

#if ZE_NAVIGATION_DEBUG
            if (NavigationLogger.Settings.HasFlag(NavigationLogEvents.FullPortalSelectionLog))
                DEBUG_LogPortalOptions(input.Request);
#endif

            var pathCost = CalculateShortestPortalsPathCommand.Execute(this, input.Request.StartHexCoord, input.Request.EndTripos);
            FinishProcess(input, pathCost);
        }

        private void FinishProcess(PortalConstructionProcessInput input, float pathCost)
        {
            ClearUnusedOutput();
            
            var resultsCopy = _resultingPath.ToArray(Allocator.Persistent);
            Output = new(
                input.ReservedPathId,
                resultsCopy,
                FormResult(input.Request, pathCost, resultsCopy.AsReadOnly()));

#if ZE_NAVIGATION_DEBUG
            if (NavigationLogger.Settings.HasFlag(NavigationLogEvents.HexPathCalculationEnd))
                UnityEngine.Debug.Log($"hex path calculated: {input.ReservedPathId}");
#endif

            _nodes.Clear();
            _activeNodeIds.Clear();
        }

        // Output contains native array which we transfer control out, so we shouldn't dispose local copy.
        // if none is use it than dispose
        public void ClearUsedOutput() => Output = default;
        private void ClearUnusedOutput() => Output.Dispose();



        private async Awaitable PreparePortalOptions(
           int startZone,
           int2 hexCoord,
           int2 targetHexCoord,
           IntTriangularPos pos,
           IPortalsList portalOptions)
        {
            _isDisposeAvailable = false;
            // 1. calculate distances map through job
            _calculateDistancesProcess.Launch(new(0, hexCoord, pos));
            do
            {
                await Awaitable.NextFrameAsync();
            }
            while (_calculateDistancesProcess.Stage == CalculationProcessStage.Calculating);
            _isDisposeAvailable = true;

            if (StopProcessRequired)
                return;


            // 2. get all accessible portals, write also shortest distance
            portalOptions.Clear();
            var directionCoefficients = HexTransitionLogic.GetDirectionCostCoefficients(hexCoord, targetHexCoord);

            _hexPortalsList.Clear();
            _portalsCoordinator.GetHexPortalExits(startZone, hexCoord, _hexPortalsList);
            var calculationDistancesResult = _calculateDistancesProcess.StopAndGetResults();
            foreach (var exitOption in _hexPortalsList)
            {
                var exitData = exitOption.ExitData;
                var edge = exitData.Edge;
                
                var minDist = float.MaxValue;
                var portalCf = directionCoefficients[edge];

                foreach (var portalTriangle in edge.GetEdgeEnumerable(exitData))
                {
                    minDist = math.min(minDist, calculationDistancesResult.GetDistance(portalTriangle) * portalCf);                    
                }

            if (minDist == float.MaxValue)
                    continue;

                portalOptions.Add(new() { MinDist = minDist, PortalId = exitOption.PortalId, ZoneIndex = exitData.ZoneIndex });
            }
        }

        private PathCalculationResult<PortalPathDestinationKey, int> FormResult(in HexPathSearchRequest request, float pathCost, NativeArray<int>.ReadOnly resultingPath)
        {
            var startKey = new PortalPathDestinationKey(request.StartHexCoord, request.StartHexZoneIndex);
            var endKey = new PortalPathDestinationKey(request.EndHexCoord, request.EndHexZoneIndex);
            return new PathCalculationResult<PortalPathDestinationKey, int>(
                start: startKey,
                end: endKey,
                readOnlyPoints: resultingPath,
                pathCost: pathCost,
                hasReachedTarget: true );
        }

        #if ZE_NAVIGATION_DEBUG
        private void DEBUG_LogPortalOptions(in HexPathSearchRequest request)
        {
            var stringBuilder = new System.Text.StringBuilder();
            stringBuilder.AppendLine($"{request.StartHexCoord} zone {request.StartHexZoneIndex} -> {request.EndHexCoord} zone {request.EndHexZoneIndex}");
            stringBuilder.AppendLine("start portals:");
            foreach (var startPortal in _startPortals)
            {
                stringBuilder.AppendLine($"{startPortal.PortalId} : {startPortal.MinDist}");
            }

            stringBuilder.AppendLine("end portals: ");
            foreach (var endPortal in _endPortals.Values)
            {
                stringBuilder.AppendLine($"{endPortal.PortalId} : {endPortal.MinDist}");
            }

            UnityEngine.Debug.Log(stringBuilder.ToString());
        }
        #endif
    }
}
