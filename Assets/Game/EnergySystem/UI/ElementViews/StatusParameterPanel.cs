using UnityEngine;
using TMPro;

namespace ZE.MechBattle
{
    public class StatusParameterPanel : MonoBehaviour
    {
        public struct UpdateProtocol
        {
            public string ParameterString;
            public int ParameterChangeSign;
        }

        [SerializeField] private TextMeshProUGUI _parameterLabel;
        [SerializeField] private GameObject _riseMarker;
        [SerializeField] private GameObject _lowerMarker;

        public void UpdateData(UpdateProtocol protocol)
        {
            _parameterLabel.text = protocol.ParameterString;
            _riseMarker.SetActive(protocol.ParameterChangeSign == 1);
            _lowerMarker.SetActive(protocol.ParameterChangeSign == -1);
        }
    }
}
