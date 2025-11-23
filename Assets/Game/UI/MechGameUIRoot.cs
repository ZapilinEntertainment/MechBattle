using UnityEngine;
using ZE.UiService;

namespace ZE.MechBattle.UI
{
    public class MechGameUIRoot : UiRoot, IUILinesParent
    {
        [field:SerializeField] public Transform LinesContainer { get;private set; }    
    }
}
