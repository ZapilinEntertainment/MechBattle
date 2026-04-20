using UnityEngine;
using System;

namespace ZE.MechBattle.Navigation
{
    public static class NavigationConstants
    {
        public const float CASTING_HEIGHT = 100f;
        public const float CASTING_RAY_LENGTH = 200f;
        public const float NAV_OBSTACLES_LOCK_PERCENT = 0.5f;
        public const int MAX_TRIANGLES_PER_EDGE = 32; // NOTE: HexEdgesAccessMap depends on it

        public const short DEFAULT_HEIGHT = 0;
        public const float MAX_HEIGHT_STEP = 5f;

        public const float EDGE_PASS_COST = 1f;
        public const float VERTEX_PASS_COST = 2.01f;
        public const float LONG_VERTEX_PASS_COST = SQRT_OF_THREE;

        //public const int PEAK_EDGES_MASK = (1 << (int)PeakNeighbour.EdgeDown) + (1 << (int)PeakNeighbour.EdgeUpLeft) + (1 << (int)PeakNeighbour.EdgeUpRight);
       // public const int PEAK_LONG_VERTEX_MASK = (1 << (int)PeakNeighbour.EdgeUpRight) + (1 << (int)PeakNeighbour.VertexRight) + (1 << (int)PeakNeighbour.VertexDownRightPeak) + ()
       // public const int VALLEY_EDGES_MASK = (1 << (int)ValleyNeighbour.EdgeDownLeft) + (1 << (int)ValleyNeighbour.EdgeDownRight) + (1 << (int)ValleyNeighbour.EdgeUp);

        public const sbyte DEFAULT_TRIANGLE_ENTRANCE_COST = 1;


        public const float SQRT_OF_THREE = 1.73205f;
        public const double SQRT_OF_THREE_DBL = 1.732050807568877;
        public const float SQRT_OF_THREE_HALVED = SQRT_OF_THREE * 0.5f;
        public const float DIV_SQRT_OF_THREE = (float)(1 / SQRT_OF_THREE_DBL);
        public const float DIV_THREE = 1f / 3f;

        public const double SQRT_THREE_D_3_DBL = SQRT_OF_THREE_DBL / 3;
        public const float SQRT_THREE_D_3 = SQRT_OF_THREE / 3f;
        
        public const int TRIANGLE_DIRECTIONS_COUNT = 12;



        public static QueryParameters GetWalkableCastQueryParameters()
        {
            var layerMask = LayerMask.GetMask("NAV_Walkable");
            return new(layerMask, false, QueryTriggerInteraction.Ignore, false);
        }

        public static QueryParameters GetObstacleCastQueryParameters()
        {
            var layerMask = LayerMask.GetMask("NAV_Obstacle");
            return new(layerMask, false, QueryTriggerInteraction.Ignore, false);
        }

    }
}
