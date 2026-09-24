using UnityEngine;
using UnityEngine.UI;

namespace ZE.MechBattle
{
    public class UIWeaponAimMarker : DisposableGameObject
    {
        public struct UpdateProtocol
        {
            public Vector3 ScreenPos;
            public float GunLoadingProgress;
            public float EnergyChargePc;
        }

        [SerializeField] private float _basicRingOffset = 0f;
        [SerializeField] private float _ringOffsetStep = 5f;
        [SerializeField] private Image _readyProgressBar;
        [SerializeField] private Image _energyProgressBar;
        [SerializeField] private Gradient _gradient;
        private Color _tint;
        public void SetVisibility(bool isVisible) => gameObject?.SetActive(isVisible);

        public void UpdateData(UpdateProtocol protocol) 
        {
            if (IsDisposed)
                return;

            transform.position = protocol.ScreenPos;

            _readyProgressBar.fillAmount = protocol.GunLoadingProgress;
            _readyProgressBar.color = _gradient.Evaluate(protocol.GunLoadingProgress) * _tint;

            _energyProgressBar.fillAmount = protocol.EnergyChargePc;
        }

        public void Setup(WeaponAimMarkerDisplayProtocol protocol)
        {
            _tint = protocol.Color;

            var offset = _basicRingOffset + _ringOffsetStep * protocol.Level;
            var offsetV2 = new Vector2(offset, offset);
            _readyProgressBar.rectTransform.offsetMin = offsetV2;
            _readyProgressBar.rectTransform.offsetMax = -offsetV2;
        }

    }
}
