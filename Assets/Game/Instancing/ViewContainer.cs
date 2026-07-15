using UnityEngine;

namespace ZE.MechBattle.Views
{
    // an object, which serves as empty container for view.
    // When entity creates, container assign immediately as his view (presenting its game world transform)
    // however its real visible view will be loaded and instances in next few frames
    // benefits:
    // - async resources load at play-time
    // - per-frame instancing can be limited
    // - no exceptions with no-view entity
    // - when visible view loads, it will be correctly synchronised to all entity components
    public class ViewContainer : MonoBehaviour, IViewContainer, IMonoView
    {
        public Transform Transform => _transform;
        private Transform _transform;
        private GameObject _gameObject;

        public IView View { get; private set; }

        // poolable
        public void Dispose() { }

        public void OnViewInstanced(IView prefab) 
        {
            View = prefab;
            View.SetParent(_transform);
        }

        public void SetParent(Transform parent) => _transform.SetParent(parent, false);
        

        public void Init()
        {
            _transform = transform;
            _gameObject = gameObject;
        }

        public void SetVisibility(bool x) => _gameObject.SetActive(x);
    }
}
