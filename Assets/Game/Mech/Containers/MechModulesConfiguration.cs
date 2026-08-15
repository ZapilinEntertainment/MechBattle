using System.Collections.Generic;

namespace ZE.MechBattle
{
    public class MechModulesConfiguration
    {
        public IReadOnlyDictionary<MechSlot, string> InstalledModules => _installedModules;
        private readonly Dictionary<MechSlot, string> _installedModules = new();

        public void FillSlot(MechSlot slot, string slotData) => _installedModules[slot] = slotData;
    }
}
