using UnityEngine;

namespace ZE.MechBattle
{
    public readonly struct TargetData
    {
        public readonly bool IsDefined;
        public readonly Vector3 Position;

        public TargetData(Vector3 position)
        {
            Position = position;
            IsDefined = true;
        }
    }
}
