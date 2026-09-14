using ZE.UiService;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

namespace ZE.MechBattle
{
    public class UILaserEyesWindow : UiWindow
    {
        public struct UpdateProtocol
        {
            public float ChargePercent;
            public float OverchargePercent;
            public WeaponChargeStatus Status;
            public string LabelText;
        }

        [SerializeField] private GameObject _panelGO;
        [SerializeField] private Image _chargeProgressBar;
        [SerializeField] private Color _initialChargeColor;
        [SerializeField] private Color _dischargeColor;
        [SerializeField] private Gradient _overchargeGradient;
        [SerializeField] private TextMeshProUGUI _progressLabel;

        public void SetVisibility(bool x) => _panelGO.SetActive(x);

        public void UpdateData(UpdateProtocol protocol)
        {
            _chargeProgressBar.fillAmount = protocol.ChargePercent;
            _progressLabel.text = protocol.LabelText;

            switch (protocol.Status)
            {
                case WeaponChargeStatus.Charge:
                    {
                        _chargeProgressBar.color = _initialChargeColor;
                        break;
                    }

                case WeaponChargeStatus.ReadyToDischarge:
                    {
                        _chargeProgressBar.color = _overchargeGradient.Evaluate(protocol.OverchargePercent);
                        break;
                    }
                case WeaponChargeStatus.Discharging:
                    {
                        _chargeProgressBar.color = _dischargeColor;
                        break;
                    }
            }
        }
    
    }
}
