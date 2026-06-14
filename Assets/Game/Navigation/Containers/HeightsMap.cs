using System;
using Unity.Mathematics;
using Unity.Collections;

namespace ZE.MechBattle.Navigation
{
    public class HeightsMap : IDisposable
    {
        private NativeParallelHashMap<IntTriangularPos, CellHeightData> _data;

        public HeightsMap (int capacity, Allocator allocator)
        {
            _data = new (capacity, allocator);
        }

        public void Dispose()
        {
#if UNITY_EDITOR
            if (ZE.Utils.EditorPlaymodeLifetimeObject.IsQuitting)
                return;
#endif  
            _data.Dispose();
        }
    }
}
