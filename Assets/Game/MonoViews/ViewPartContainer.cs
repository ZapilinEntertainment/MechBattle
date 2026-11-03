using UnityEngine;

namespace ZE.MechBattle
{
    // simple container-wrapper for transferring mech part transforms
    public class ViewPartContainer : IViewPart, IViewConnectionsPoint
    {
        public Transform Transform => _transform;
        public GameObject GameObject => _transform.gameObject;
        private readonly Transform _transform;

        public ViewPartContainer(Transform transform) => _transform = transform;

        public void Dispose() => GameObject.Destroy(_transform.gameObject);

        public void SetParent(Transform parent)
        {
            _transform.parent = parent;
            _transform.SetLocalPositionAndRotation(Vector3.zero, Quaternion.identity);
        }
    }
}
