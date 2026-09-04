using UnityEngine;
using ZE.UiService;

namespace ZE.MechBattle
{
    public class UIMechInterfaceWindow : UiWindow
    {
        public enum MechInterfaceSubwindow : byte { Undefined, Partitions}

        public Transform GetParent(MechInterfaceSubwindow subwindow) => transform;
    }
}
