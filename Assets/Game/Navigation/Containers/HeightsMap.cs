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
            try
            {
                FinalDispose();
            }
            catch (Exception ex)
            {
                if (!ZE.Utils.EditorPlaymodeLifetimeObject.IsQuitting)
                    UnityEngine.Debug.LogError(ex);
            }
            return;
#else  

            FinalDispose();       
#endif  
        }

        private void FinalDispose()
        {
            _data.Dispose();
        }
    }
}
