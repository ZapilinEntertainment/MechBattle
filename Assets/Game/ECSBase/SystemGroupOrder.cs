using UnityEngine;

namespace ZE.MechBattle
{
    // note that is not similar to unity events order. In case of using events, use suitable interfaces: ex. IFixedSystem
    public enum SystemGroupOrder : byte { Initialization = 0, Default = 1, RegularUpdate, PostUpdate, Final }
}
