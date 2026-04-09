using System;
using System.Threading;
using Unity.Collections;
using Unity.Jobs;
using UnityEngine;

namespace ZE.MechBattle.Navigation
{
    public static class CombineFlowMapsCommand
    {
        private readonly struct Logic : IDisposable
        {
            private readonly NativeArray<FlowFieldCellCalculationData> _calculationData;
            private readonly int _length;
            private readonly int _radius;
            private readonly int _trianglesInHex;
            private readonly FlowFieldCalculationCollections _data;
            private readonly TrianglesToIndexConverter _coordsConverter;
            private readonly SquaredHexTrianglesList<TriangleNavData> _setupData;            
            private readonly NavigationHexPosition _hexPos;

            private readonly CombinedFlowMapCellsStorage _compositeMap;
            private readonly DisposableArray<int> _hexTriangleIndices;

            public Logic(FlowFieldCalculationCollections data, INavigationCaster caster, NavigationHexPosition hexPos)
            {
                _data = data;
                _setupData = _data.SetupData;
                _calculationData = data.CalculationData;
                _length = _setupData.Length;
                _coordsConverter = _setupData.CoordsConverter;
                _radius = caster.TrianglesPerHexEdge;
                _hexPos = hexPos;

                _compositeMap = new CombinedFlowMapCellsStorage(_length, _setupData.CoordsConverter);
                _trianglesInHex = caster.HexTrianglesCount;
                _hexTriangleIndices = new DisposableArray<int>(_trianglesInHex);
                var ti = 0;
                foreach (var hexTrianglePos in new HexTrianglesEnumerator(hexPos, _radius))
                {
                    var index = _coordsConverter.TriangularToIndex(hexTrianglePos);
                    _hexTriangleIndices[ti++] = index;
                }
            }

            public JobHandle ScheduleJob(HexEdge edge)
            {
                var job = new GenerateFlowFieldJob()
                {
                    SetupData = _setupData,
                    CalculationData = _calculationData,
                    HexData = _hexPos,
                    CalculationQueue = _data.CalculationQueue,
                    QueuedPositions = _data.QueuedPositions,
                    ExitEdge = edge,
                    TrianglesPerEdge = _radius
                };
                return job.ScheduleByRef();
            }

            public void UpdateCompositeMap(HexEdge edge)
            {
                for (var i = 0; i < _trianglesInHex; i++)
                {
                    var index = _hexTriangleIndices[i];

                    var defaultData = _setupData[index];
                    if (!defaultData.IsValid)
                        continue;

                    var calculatedData = _calculationData[index];
                    var cellData = new FlowMapCellData(direction: calculatedData.FlowDirection, exitDistance: (ushort)calculatedData.IntegrationValue);
                    _compositeMap.SetValue(edge, index, cellData);
                }
            }

            public NativeHashMap<IntTriangularPos, FlowMapCombinedCell> FormResultsMap(Allocator allocator)
            {
                var resultingData = new NativeHashMap<IntTriangularPos, FlowMapCombinedCell>(_trianglesInHex, allocator);
                for (var i = 0; i < _trianglesInHex; i++)
                {
                    var index = _hexTriangleIndices[i];
                    var triangleSetupData = _setupData[index];
                    if (!triangleSetupData.IsValid)
                        continue;

                    var compositeCell = _compositeMap.GetCombinedCell(index, triangleSetupData);
                    resultingData.Add(_coordsConverter.IndexToTriangular(index), compositeCell);
                }
                return resultingData;
            }

            public void Dispose()
            {
                _compositeMap.Dispose();
                _hexTriangleIndices.Dispose();
            }

        }

        public static async Awaitable<NativeHashMap<IntTriangularPos, FlowMapCombinedCell>> ExecuteAsync(
               FlowFieldCalculationCollections data,
               NavigationHexPosition hexPos,
               INavigationCaster caster,
               Allocator allocator,
               CancellationToken cancellationToken)
        {
            using var logic = new Logic(data, caster, hexPos);

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
                    return default;

                logic.UpdateCompositeMap(edge);
            }            

            return logic.FormResultsMap(allocator);
        }

        public static NativeHashMap<IntTriangularPos, FlowMapCombinedCell> Execute(
              FlowFieldCalculationCollections data,
              NavigationHexPosition hexPos,
              INavigationCaster caster,
              Allocator allocator)
        {
            using var logic = new Logic(data, caster, hexPos);
            for (var e = 0; e < 6; e++)
            {
                var edge = (HexEdge)e;
                var handle = logic.ScheduleJob(edge);
                handle.Complete();

                logic.UpdateCompositeMap(edge);
            }

            return logic.FormResultsMap(allocator);
        }
    }
}
