using System;
using UnityEngine;
using Unity.Mathematics;

namespace ZE.MechBattle
{
    [Serializable]
    public struct StepSettings
    {
        public float Duration;
        public float StepRaiseHeight;
        public float MaxSteerAngle;
        [Range(0, 0.99f)] public float DefaultChassisHeight;// = 0.93f;
        [Range(0, 0.99f)] public float MinStepChassisHeight;// = 0.9f;
        [Range(0.1f, 1f)] public float StepLengthCf;// = 1f;
        [Range(0f, 1f)] public float ChassisMovementShiftPc; // how close the chassis root wil go to active leg pos

        public float EvaluateHeightCf(float pc) => 4 * pc * (1 - pc);
        public float EvaluateSpeedCf(float pc) => pc * pc;
        public float EvaluateChassisHorizontalShift(float pc) => pc * pc * ChassisMovementShiftPc;
    }
}
