using UnityEngine;

namespace ZE.MechBattle
{
    // simple container-wrapper for transferring mech part transforms
    public class ViewPartContainer : IViewPart, IViewConnectionsPoint
    {
        public Transform Transform => _transform;
        private readonly Transform _transform;

        #if UNITY_EDITOR
        public string name { get => _transform.gameObject.name; set => _transform.gameObject.name = value; }
        #endif

        public ViewPartContainer(Transform transform) => _transform = transform;

        public void Dispose() { }

        public void SetParent(Transform parent)
        {
            _transform.parent = parent;
            _transform.SetLocalPositionAndRotation(Vector3.zero, Quaternion.identity);
        }
    }
}
