using UnityEngine.Pool;
using System.Collections.Generic;
using System;

namespace ZE.MechBattle
{
    public class PortalExitsUpdateData 
    {
        public List<NavigationPortalExit> ExitsA = new(8);
        public List<NavigationPortalExit> ExitsB = new(8);
        private readonly IObjectPool<PortalExitsUpdateData> _pool;

        public PortalExitsUpdateData(IObjectPool<PortalExitsUpdateData> pool)
        {
            _pool = pool;
        }

        public void Clear()
        {
            ExitsA.Clear();
            ExitsB.Clear();
        }        

        public void ReturnToPool() => _pool.Release(this);
    }

    public class PortalExitsUpdateDataPool 
    {
        private ObjectPool<PortalExitsUpdateData> _pool;

        public PortalExitsUpdateDataPool()
        {
            _pool = new(createFunc: () => new(_pool), actionOnGet: (data) => data.Clear(), defaultCapacity: 4, maxSize: 128); 
        }

        public PortalExitsUpdateData Get() => _pool.Get();
    }
}
