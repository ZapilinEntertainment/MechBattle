using System;
using Unity.Mathematics;

namespace ZE.MechBattle.HexCapturing
{
    public readonly struct HexDataKey : IEquatable<HexDataKey>
    {
        public readonly int PlayerId;
        public readonly int2 HexCoord;

        public HexDataKey(int2 hexCoord, int playerId)
        {
            PlayerId = playerId;
            HexCoord = hexCoord;
        }

        public bool Equals(HexDataKey other)
        {
            return PlayerId == other.PlayerId && HexCoord.Equals(other.HexCoord);
        }

        public override bool Equals(object obj)
        {
            return obj is HexDataKey other && Equals(other);
        }

        public override int GetHashCode()
        {
            unchecked
            {
                int hash = 17;
                hash = hash * 31 + PlayerId;
                hash = hash * 31 + HexCoord.x;
                hash = hash * 31 + HexCoord.y;
                return hash;
            }
        }
    }
}
