using System;
using System.Threading;
using Unity.Collections;
using Unity.Jobs;
using UnityEngine;

namespace ZE.MechBattle.Navigation
{
    public static class GenerateAndCombineFlowMapsCommand
    {
        private readonly struct Logic : IDisposable
        {
            private readonly bool _exitNeighbourCheckRequired;
            private readonly NativeArray<FlowFieldCellCalculationData> _calculationData;
            private readonly int _length;
            private readonly int _radius;
            private readonly int _trianglesInHex;
            private readonly FlowFieldCalculationCollections _data;
            private readonly FlattenedHexList<CellPassabilityData> _setupData;            
            private readonly NavigationHexPosition _hexPos;

            private readonly CombinedFlowMapCellsStorage _compositeMap;

            public Logic(FlowFieldCalculationCollections data, NavigationHexPosition hexPos, int hexRadius, bool exitNeighbourCheckRequired)
            {
                _data = data;
                _setupData = _data.PassabilityData;
                _calculationData = data.CalculationData;
                _length = _setupData.Length;
                _radius = hexRadius;
                _hexPos = hexPos;
                _exitNeighbourCheckRequired = exitNeighbourCheckRequired;

                _compositeMap = new CombinedFlowMapCellsStorage(_length, _setupData.GetCoordsConverter());
                _trianglesInHex = TriangularMath.GetTrianglesCountInHex(_radius);
            }

            public JobHandle ScheduleJob(HexEdge edge)
            {
                var job = new GenerateFlowFieldJob()
                {
                    PassabilityData = _setupData,
                    CalculationData = _calculationData,
                    HexData = _hexPos,
                    CalculationQueue = _data.CalculationQueue,
                    QueuedPositions = _data.QueuedPositions,
                    ExitEdge = edge,
                    TrianglesPerEdge = _radius,
                    ExitNeighbourPassabilityRequired = _exitNeighbourCheckRequired
                };
                return job.ScheduleByRef();
            }

            public void UpdateCompositeMap(HexEdge edge)
            {
                for (var i = 0; i < _trianglesInHex; i++)
                {
                    var calculatedData = _calculationData[i];
                    var cellData = new FlowMapCellData(direction: calculatedData.FlowDirection, exitDistance: (ushort)calculatedData.IntegrationValue);
                    _compositeMap.SetValue(edge, i, cellData);
                }
            }

            public void FillResultsMap()
            {
                var flowData = _data.FlowData;
                for (var i = 0; i < _trianglesInHex; i++)
                {
                    flowData[i] = _compositeMap.GetCombinedCell(i);
                }
            }

            public void Dispose()
            {
                _compositeMap.Dispose();
            }

        }

        public static async Awaitable ExecuteAsync(
               FlowFieldCalculationCollections data,
               NavigationHexPosition hexPos,
               int hexRadius,
               bool exitNeighbourCheckRequired,
               CancellationToken cancellationToken)
        {
            using var logic = new Logic(data, hexPos, hexRadius, exitNeighbourCheckRequired);

            for (var e = 0; e < 6; e++)
            {
                var edge = (HexEdge)e;
                var handle = logic.ScheduleJob(edge);
                while (!handle.IsCompleted)
                {
                    await Awaitable.NextFrameAsync();
                }
                handle.Complete();

                if (cancellationToken.IsCancellationRequested)
                    return;

                logic.UpdateCompositeMap(edge);
            }            

            logic.FillResultsMap();
        }

        public static void Execute(
              FlowFieldCalculationCollections data,
              NavigationHexPosition hexPos,              
              int hexRadius,
              bool exitNeighbourCheckRequired)
        {
            using var logic = new Logic(data, hexPos, hexRadius, exitNeighbourCheckRequired);
            for (var e = 0; e < 6; e++)
            {
                var edge = (HexEdge)e;
                var handle = logic.ScheduleJob(edge);
                handle.Complete();

                logic.UpdateCompositeMap(edge);
            }

            logic.FillResultsMap();
        }
    }
}
