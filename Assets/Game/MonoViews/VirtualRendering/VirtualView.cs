using UnityEngine;
using ZE.MechBattle.Ecs;

namespace ZE.MechBattle
{
    public class VirtualView : IVirtualView
    {
        public VirtualViewComponent ViewData => _value;
        private readonly VirtualViewComponent _value;

        public VirtualView(VirtualViewComponent value) => _value = value;

        public void Dispose() { }
    }
}
