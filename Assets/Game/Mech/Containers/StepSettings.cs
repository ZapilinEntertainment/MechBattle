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
        public float VerticalWobblingHeight;
        [Range(0, 0.99f)] public float DefaultChassisHeight;// = 0.93f;
        [Range(0, 0.99f)] public float MinStepChassisHeight;// = 0.9f;
        [Range(0.1f, 1f)] public float StepLengthCf;// = 1f;
        [Range(0f, 1f)] public float ChassisMovementShiftPc; // how close the chassis root wil go to active leg pos

        public float MinStepRadiusCf; // 1.5f, will be multiplied by hips distance
        public float MaxStepRadiusCf; // 2f, will be multiplied by hips distance
        public float MaxZOffsetCf; // 0.7f, will be multiplied by miin step distance

        public float EvaluateHeightCf(float pc) => 4 * pc * (1 - pc);
        public float EvaluateSpeedCf(float pc) => pc * pc;
        public float EvaluateChassisHorizontalWobbling(float pc) => pc * pc ;
        public float EvaluateChassisVerticalWobbling(float pc) => math.sin(pc * math.PI);
    }
}
