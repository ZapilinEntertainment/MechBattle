using UnityEngine;
using Unity.Mathematics;

namespace ZE.MechBattle.Navigation
{
    public class NavigationMapController
    {
        private readonly INavigationMap _map;
        private const float COS_60 = 0.5f;
        private readonly float2 RIGHT = new(1f,0f);
        private float3[] _peakDirections;
        private float3[] _valleyDirections;

        public NavigationMapController(INavigationMap map)
        {
            _map = map;

            _peakDirections = new float3[12];
            _valleyDirections = new float3[12];
            for (byte i = 0; i < 12; i++)
            {
                _peakDirections[i] = TriangularMath.TriangularDirectionToWorld(i, true);
                _valleyDirections[i] = TriangularMath.TriangularDirectionToWorld(i, false);
            }
        }

        public bool TryGetNavigationMoveDirection(in float3 pos, in float3 targetPos, out float3 direction)
        {
            //var hex = TriangularMath.WorldToHex(pos.xz, _map.HexEdgeSize);
            //if (!_map.ContainsHex(hex))
            //{
            //    // out of navigation map
            //    var nearestHex = _map.GetNearestHex(pos.xz);
            //    direction = math.normalize(nearestHex.CenterPos3D - pos);
            //    return true;
            //}

            //var dir = math.normalize(targetPos - pos).xz;
            //var exitEdge = DefineExitEdge(dir);
            //if (!_map.TryGetFlowMap(hex, exitEdge, out var map))
            //{
            //    // wait until map is ready
            //    RequestFlowMap(hex, exitEdge);
            //    direction = float3.zero;
            //    return false;
            //}               

            //var triangle = TriangularMath.WorldToTrianglePos(pos, _map.TriangleEdgeSize).ToStandartized();
            //if (map.TryGetFlowDirection(triangle, out var byteDir))
            //{
            //    direction = triangle.IsPeak ? _peakDirections[byteDir] : _valleyDirections[byteDir];
            //    return true;
            //}
            //else
            //{
            //    direction = float3.zero;
            //    return false;
            //}
            direction = default;
            return false;
        }

        private HexEdge DefineExitEdge(in float2 dir)
        {            
            var dot = math.dot(RIGHT, dir);
            HexEdge exitEdge;
            var isPositive = dir.y > 0f;
            if (dot > 0.5f)
            {
                exitEdge = isPositive ? HexEdge.TopRight : HexEdge.BottomRight;
            }
            else
            {
                if (dot < -0.5f)
                    exitEdge = isPositive ? HexEdge.TopLeft : HexEdge.BottomLeft;
                else
                    exitEdge = isPositive ? HexEdge.Top : HexEdge.Bottom;
            }
            return exitEdge;
        }

        private void RequestFlowMap(int2 hex, HexEdge exitEdge)
        {

        }
    
    }
}
