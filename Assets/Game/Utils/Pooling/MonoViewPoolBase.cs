using System;
using UnityEngine;

namespace ZE.Utils
{
    public abstract class MonoViewPoolBase<T> : PoolBase<T> where T : MonoBehaviour, IPoolableObject<T>, IDisposable
    {
        private readonly Transform _hostObject;        
        private readonly T _prefab;   

        public MonoViewPoolBase(T prefab, Transform hostOfPools, string poolName, int defaultCapacity = 4, int maxCapacity = 1024) : base(defaultCapacity, maxCapacity)
        {
            _prefab = prefab;
            _hostObject = new GameObject(poolName).transform;
            GameObject.DontDestroyOnLoad(_hostObject);
            _hostObject.parent = hostOfPools;
        }

        public void MarkAsDontDestroy() => GameObject.DontDestroyOnLoad(_hostObject.gameObject);

        protected override T Instantiate() => GameObject.Instantiate(_prefab, _hostObject);

        protected override void OnRelease(T instance)
        {
            base.OnRelease(instance);

            if (instance.gameObject != null)
                instance.gameObject.SetActive(false);         
            instance.transform.SetParent(_hostObject);            
        }


        protected override void OnGet(T instance)
        {
            base.OnGet(instance);
            instance.gameObject.SetActive(true);
        }

        public override void Dispose()
        {
            base.Dispose();

            if (_hostObject != null)
            {
                GameObject.Destroy(_hostObject.gameObject);
            }           
        }
    }
}
