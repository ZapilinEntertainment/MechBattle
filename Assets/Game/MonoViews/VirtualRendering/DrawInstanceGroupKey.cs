using System;

namespace ZE.MechBattle.Views
{
    public readonly struct DrawInstanceGroupKey : IEquatable<DrawInstanceGroupKey>
    {
        public readonly int MaterialId;
        public readonly int MeshId;

        public DrawInstanceGroupKey(int materialId, int meshId)
        {
            MaterialId = materialId;
            MeshId = meshId;
        }

        public bool Equals(DrawInstanceGroupKey other)
   => MaterialId == other.MaterialId && MeshId == other.MeshId;

        public override bool Equals(object? obj)
            => obj is DrawInstanceGroupKey other && Equals(other);

        public override int GetHashCode()
            => HashCode.Combine(MaterialId, MeshId);

        public static bool operator ==(DrawInstanceGroupKey a, DrawInstanceGroupKey b) => a.Equals(b);
        public static bool operator !=(DrawInstanceGroupKey a, DrawInstanceGroupKey b) => !a.Equals(b);
    }
}
