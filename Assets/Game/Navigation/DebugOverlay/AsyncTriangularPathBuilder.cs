using System;
using System.Threading;
using System.Threading.Tasks;
using System.Collections.Generic;
using UnityEngine;
using Unity.Mathematics;
using Unity.Jobs;
using Unity.Collections;

namespace ZE.MechBattle.Navigation.DebugOverlay
{
    public class AsyncTriangularPathBuilder : TriangularPathBuilderBase
    {
        private readonly CancellationTokenSource _cancellationTokenSource = new();
        private bool _isJobExecuting = false;

        public AsyncTriangularPathBuilder(INavigationMap map) : base(map) { }

        public async Task<Result> BuildAsync(
            IntTriangularPos startPos, 
            IntTriangularPos endPos,
            CancellationToken cancellationToken)
        {
            var points = new List<IntTriangularPos>();
            var startHex = TriangularMath.TriangularToHex(startPos, _map.TriangleHeight, _map.HexEdgeSize);
            var endHex = TriangularMath.TriangularToHex(endPos, _map.TriangleHeight, _map.HexEdgeSize);

            if (math.all(startHex == endHex))
            {
                //single hex
                CalculateTrianglePath(startHex, startPos, endPos);

                /*  DEBUG LOG
                var coordsConverter = _triangularPathJobData.SetupData.CoordsConverter;
                for (var j = 0; j < _triangularPathJobData.CalculationData.Length; j++)
                {
                    var setupData = _triangularPathJobData.SetupData[j];
                    if (!setupData.IsValid)
                        continue;

                    Debug.Log($"{coordsConverter.IndexToTriangular(j)}: {_triangularPathJobData.CalculationData[j].PathCost}");
                }
                */

                foreach (var point in _triangularPathJobData.ResultList)
                {
                    points.Add(point);
                }

                return new(points);
            }

            // CALCULATE HEX PATH   
            _isJobExecuting = true;
            GetShortestHexPathCommand.PathfindResult result = default;
            using var combinedSource = CancellationTokenSource.CreateLinkedTokenSource(_cancellationTokenSource.Token, cancellationToken);
            CancellationToken combinedToken = combinedSource.Token;
            try            
            {                
                result = await GetShortestHexPathCommand.ExecuteAsync(startPos, endPos, _map, GetHexPathJobData(), combinedToken);
                combinedSource.Dispose();
            }
            catch (Exception ex)
            {
                Debug.LogError("hex pathfinding failed: " + ex.ToString());
            }
            finally
            {
                _isJobExecuting = false;
            }

            if (combinedToken.IsCancellationRequested)
                return default;

            if (!result.IsSuccess)
                return new(ResultCode.CannotBuildHexPath);

            var hexNodes = result.Path;
            var transitionsCount = hexNodes.Count;
            if (transitionsCount == 0)
                return new(ResultCode.InvalidHexPath);

            var prevPos = startPos;
            for (var i = 0; i < hexNodes.Count; i++)
            {
                prevPos = AddPathTriangles(prevPos, hexNodes[i], points);
            }


            
            // last part: edge transition (inside last hex) -> final pos
            _isJobExecuting = true;
            try
            {
                CalculateTrianglePath(endHex, prevPos, endPos);
            }
            catch (Exception ex)
            {
                Debug.LogError("triangle pathfinding failed: " + ex.ToString());
            }
            finally
            {
                _isJobExecuting = false;
            }

            if (combinedToken.IsCancellationRequested)
                return default;

            var count = _triangularPathJobData.ResultList.Length;
            for (var i = 1; i < count; i++)
            {
                points.Add(_triangularPathJobData.ResultList[i]);
            }

            return new(points);
        }

        public override void Dispose()
        {
            _cancellationTokenSource.Cancel();
            _cancellationTokenSource.Dispose();

            if (_isJobExecuting)
                DisposeAsync();
            else
                FinalDispose();
        }

        private async void DisposeAsync()
        {
            using var cts = new CancellationTokenSource(TimeSpan.FromSeconds(30));

            try
            {
                Debug.LogWarning("job calculations still not finished, waiting for complete...");
                var token = cts.Token;
                do
                {
                    await Awaitable.NextFrameAsync();
                }
                while (_isJobExecuting & !token.IsCancellationRequested);
            }
            catch (OperationCanceledException)
            {
                Debug.LogWarning("dispose timeout! Did you forget to set async flag to false?");
            }
            catch (Exception ex)
            {
                Debug.LogException(ex);
            }
            finally
            {
                FinalDispose();
                Debug.LogWarning("triangular path builder disposed");
            }
        }
    }
}
