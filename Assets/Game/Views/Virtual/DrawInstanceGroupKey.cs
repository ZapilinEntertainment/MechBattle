using System;

namespace ZE.MechBattle.Views
{
    public readonly struct DrawInstanceGroupKey : IEquatable<DrawInstanceGroupKey>
    {
        public readonly int MaterialId;
        public readonly int MeshId;
        public readonly int SubmeshIndex;

        public DrawInstanceGroupKey(int materialId, int meshId, int submeshIndex = 0)
        {
            MaterialId = materialId;
            MeshId = meshId;
            SubmeshIndex = 0;
        }

        public bool Equals(DrawInstanceGroupKey other)
   => MaterialId == other.MaterialId && MeshId == other.MeshId && SubmeshIndex == other.SubmeshIndex;

        public override bool Equals(object? obj)
            => obj is DrawInstanceGroupKey other && Equals(other);

        public override int GetHashCode()
            => HashCode.Combine(MaterialId, MeshId, SubmeshIndex);

        public static bool operator ==(DrawInstanceGroupKey a, DrawInstanceGroupKey b) => a.Equals(b);
        public static bool operator !=(DrawInstanceGroupKey a, DrawInstanceGroupKey b) => !a.Equals(b);
    }
}
