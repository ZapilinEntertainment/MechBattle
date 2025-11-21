using UnityEngine;
using R3;

namespace ZE.MechBattle
{
    public interface ITargetDesignator
    {
        ReadOnlyReactiveProperty<TargetData> TargetDataProperty { get; }
    }
}
