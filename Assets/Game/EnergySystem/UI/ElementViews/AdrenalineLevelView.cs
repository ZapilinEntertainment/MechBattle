using UnityEngine;
using UnityEngine.UI;

namespace ZE.MechBattle
{
    public class AdrenalineLevelView : MonoBehaviour
    {
        [SerializeField] private Image _image;
        [SerializeField] private Color _disabledColor = Color.black;
        private Color _activeColor;

        public void SetActiveColor(Color color) => _activeColor = color;
        public void SwitchStepActivity(bool x) => _image.color = x ? _activeColor : _disabledColor;
    }
}
