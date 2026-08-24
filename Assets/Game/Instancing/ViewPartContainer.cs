using UnityEngine;

namespace ZE.MechBattle
{
    public class ViewPartContainer : IViewPart, IViewConnectionsPoint
    {
        public Transform Transform => _transform;
        private readonly Transform _transform;

        #if UNITY_EDITOR
        public string name { get => _transform.gameObject.name; set => _transform.gameObject.name = value; }
        #endif

        public ViewPartContainer(Transform transform) => _transform = transform;

        // it is just a container
        public void Dispose() { }

        public void SetParent(Transform parent) => _transform.SetParent(parent, false);
    }
}
