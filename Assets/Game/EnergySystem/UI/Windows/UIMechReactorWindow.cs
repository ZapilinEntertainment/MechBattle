using Unity.Mathematics;
using UnityEngine;
using UnityEngine.UI;
using ZE.UiService;

namespace ZE.MechBattle
{
    public class UIMechReactorWindow : UiWindow
    {
        public struct UpdateProtocol
        {
            public float ReactorConditionPc;
            public float AdrenalineLevelPc;

            public int EnergySurplus;
            public int RepairSpeed;

            public int EnergyBoostSignValue;
            public int RepairBoostSignValue;
        }

        [SerializeField] private Image _reactorConditionFillImage;
        [Space]
        [SerializeField] private RectTransform _adrenalinePanelHost;
        [SerializeField] private AdrenalineLevelView _adrenalineLevelViewPrefab;
        [SerializeField] private Gradient _adrenalineStepsColor;
        [Space]
        [SerializeField] private StatusParameterPanel _energyParameterPanel;
        [SerializeField] private StatusParameterPanel _repairParameterPanel;

        private int _adrenalineLevelsCount;
        private AdrenalineLevelView[] _adrenalineLevelViews;
        private const string PARAMETER_VALUE_FORMAT = "+0;-#;0";

        public void SetupAdrenalineSteps(int stepsCount)
        {
            if (_adrenalineLevelViews == null)
            {
                _adrenalineLevelViews = new AdrenalineLevelView[stepsCount];
                for (var i = 0; i < stepsCount; i++)
                {
                    _adrenalineLevelViews[i] = InstantiateLevelStepView();
                }
            }
            else
            {
                FitAdrenalineStepsArray(stepsCount);
            }
            _adrenalineLevelsCount = stepsCount;

            for (var i = 0; i < stepsCount; i++)
            {
                _adrenalineLevelViews[i].SetActiveColor(_adrenalineStepsColor.Evaluate(1f- (i / (float)stepsCount)));
            }
        }

        public void UpdateData(UpdateProtocol protocol)
        {
            _reactorConditionFillImage.fillAmount = protocol.ReactorConditionPc;
            SetAdrenalineLevel(protocol.AdrenalineLevelPc);
            _energyParameterPanel.UpdateData(new()
            {
                ParameterChangeSign = protocol.EnergyBoostSignValue,
                ParameterString = protocol.EnergySurplus.ToString(PARAMETER_VALUE_FORMAT)
            });

            _repairParameterPanel.UpdateData(new()
            {
                ParameterChangeSign = protocol.RepairBoostSignValue,
                ParameterString = protocol.RepairSpeed.ToString(PARAMETER_VALUE_FORMAT)
            });
        }

        private AdrenalineLevelView InstantiateLevelStepView() => GameObject.Instantiate(_adrenalineLevelViewPrefab, _adrenalinePanelHost);

        private void FitAdrenalineStepsArray(int stepsCount)
        {
            if (_adrenalineLevelViews.Length < stepsCount)
            {
                var newList = new AdrenalineLevelView[stepsCount];
                _adrenalineLevelViews.CopyTo(newList, 0);
                for (var i = _adrenalineLevelViews.Length; i < stepsCount; i++)
                {
                    newList[i] = InstantiateLevelStepView();
                }
                _adrenalineLevelViews = newList;
            }

            var count = 0;
            for (var i = stepsCount - 1; i > -1; i--)
            {
                _adrenalineLevelViews[i].gameObject.SetActive(count < _adrenalineLevelViews.Length);
                count++;
            }
        }

        private void SetAdrenalineLevel(float adrenalineVolumePc)
        {
            var j = 1;
            for (var i = _adrenalineLevelsCount-1; i > -1 ; i--)
            {
                var pc = (j++) / (float)_adrenalineLevelsCount;
                _adrenalineLevelViews[i].SwitchStepActivity(pc <= adrenalineVolumePc);
            }
        }
    }
}
