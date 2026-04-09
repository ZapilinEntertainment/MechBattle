using System;
using System.Collections.Generic;
using UnityEngine;
using Unity.Mathematics;

namespace ZE.MechBattle.Navigation.DebugOverlay
{
    public class TriangularPathBuilder : TriangularPathBuilderBase
    {
        public TriangularPathBuilder(INavigationMap map) : base(map) { }

        public Result Build(
           IntTriangularPos startPos,
           IntTriangularPos endPos)
        {
            var points = new List<IntTriangularPos>();
            var startHex = TriangularMath.TriangularToHex(startPos, _map.TriangleHeight, _map.HexEdgeSize);
            var endHex = TriangularMath.TriangularToHex(endPos, _map.TriangleHeight, _map.HexEdgeSize);

            if (math.all(startHex == endHex))
            {
                CalculateTrianglePath(startHex, startPos, endPos);
                foreach (var point in _triangularPathJobData.ResultList)
                {
                    points.Add(point);
                }

                return new(points);
            }

            // CALCULATE HEX PATH   
            GetShortestHexPathCommand.PathfindResult result = default;
            try
            {
                result = GetShortestHexPathCommand.Execute(startPos, endPos, _map, GetHexPathJobData());
            }
            catch (Exception ex)
            {
                Debug.LogError("hex pathfinding failed: " + ex.ToString());
            }

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

            try
            {
                CalculateTrianglePath(endHex, prevPos, endPos);
            }
            catch (Exception ex)
            {
                Debug.LogError("triangle pathfinding failed: " + ex.ToString());
            }

            var count = _triangularPathJobData.ResultList.Length;
            for (var i = 1; i < count; i++)
            {
                points.Add(_triangularPathJobData.ResultList[i]);
            }

            return new(points);
        }
    }
}
