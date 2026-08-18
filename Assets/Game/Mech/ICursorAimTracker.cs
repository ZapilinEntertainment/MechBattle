using UnityEngine;
using R3;

namespace ZE.MechBattle
{
    public interface ICursorAimTracker
    {
        ReadOnlyReactiveProperty<TargetData> TargetDataProperty { get; }
    }
}
