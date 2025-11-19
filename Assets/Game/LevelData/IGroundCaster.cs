using UnityEngine;

namespace ZE.MechBattle
{
    // note: there can also be virtual casters for small enemies (read directly from heights map instead of physics cast)
    // also note: ground is single-floor plane)
    public interface IGroundCaster
    {
        public struct GroundPoint
        {
            public Vector3 Position;
            public Vector3 Normal;
        }

        bool TryGetGroundPoint(float x, float z, out GroundPoint point) ;
    
    }
}
