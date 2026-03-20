using UnityEngine;

namespace ZE.MechBattle.Navigation
{
    public static class NavigationConstants
    {
        public const float CASTING_HEIGHT = 100f;
        public const float CASTING_RAY_LENGTH = 200f;
        public const float NAV_OBSTACLES_LOCK_PERCENT = 0.5f;
        public const int MAX_TRIANGLES_PER_EDGE = 32; // NOTE: HexEdgesAccessMap depends on it

        public const float EDGE_PASS_COST = 1f;
        public const float VERTEX_PASS_COST = 2f;

        public const float SQRT_OF_THREE = 1.73205f;
        public const double SQRT_OF_THREE_DBL = 1.732050807568877;

        // triangle height * 2/3 (grid step)
        public const float EDGE_TO_PARTIAL_HEIGHT_CF = 0.5773502f;

        public const double SQRT_THREE_D_3_DBL = SQRT_OF_THREE_DBL / 3;
        public const float SQRT_THREE_D_3 = SQRT_OF_THREE / 3f;

        public static QueryParameters GetGroundCastQueryParameters()
        {
            // TODO: bind to LayerConstants
            var layerMask = LayerMask.GetMask("Ground");
            return new(layerMask, false, QueryTriggerInteraction.Ignore, false);
        }
    
    }
}
