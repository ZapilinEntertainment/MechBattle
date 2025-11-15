using System;
using UnityEngine;

namespace ZE.MechBattle.Movement
{
    [Serializable]
    public class StepSettings
    {
        [field: SerializeField] public float Duration { get; private set; }
        [field: SerializeField] public float StepRaiseHeight { get; private set; }
        [field: SerializeField] public AnimationCurve SpeedCurve { get; private set; }
        [field: SerializeField] public AnimationCurve HeightCurve { get; private set; }
    }
}
