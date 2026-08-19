using UnityEngine;

namespace ZE.MechBattle
{
    public class RayEffectView : MonoBehaviour, IDisposableRayEffectView, IPoolableObject<RayEffectView>
    {
        [SerializeField] private LineRenderer _lineRenderer;
        [SerializeField] private Transform _endEffect;
        private bool _isDestroyed = false;

        public Vector3 Start { get => _lineRenderer.GetPosition(0); set => _lineRenderer.SetPosition(0, value); }
        public Vector3 End { get => _lineRenderer.GetPosition(1); set
            {
                _lineRenderer.SetPosition(1, value);
                _endEffect.position = value;
            } }

        private PoolElementReleaser<RayEffectView> _releaser;

        public void AssignReleaser(PoolElementReleaser<RayEffectView> releaser) => _releaser = releaser;

        public void Dispose() => _releaser.Release(this);

        public void SetEndEffectActivity(bool isVisible)
        {
            _endEffect.gameObject.SetActive(isVisible);
        }

        public void OnGet()
        {
            if (_isDestroyed)
                return;
            gameObject.SetActive(true);
        }
        public void OnRelease()
        {
            if (_isDestroyed)
                return;
            gameObject.SetActive(false);
        }

        private void OnDestroy()
        {
            _isDestroyed = true;
        }
    }
}
