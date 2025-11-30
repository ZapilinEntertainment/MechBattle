using UnityEngine;

namespace ZE.MechBattle
{
    public static class LayerConstants
    {
        public static readonly int FootPlacementMask = LayerMask.GetMask(GROUND_LAYER_NAME);
        public static readonly int AimCastMask = LayerMask.GetMask(DEFAULT_LAYER_NAME, GROUND_LAYER_NAME);
        public static readonly int ProjectilesCastMask = LayerMask.GetMask(DEFAULT_LAYER_NAME, GROUND_LAYER_NAME);

        private const string GROUND_LAYER_NAME = "Ground";
        private const string DEFAULT_LAYER_NAME = "Default";
    }
}
