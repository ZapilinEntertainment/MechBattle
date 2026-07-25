using UnityEngine;
using Unity.Mathematics;

namespace ZE.MechBattle
{
    public static class CalculateChassisSettingsCommand
    {
        public static ChassisSettings Execute(LegDataContainer<Transform> leftLeg, LegDataContainer<Transform> rightLeg, StepSettings stepSettings)
        {
            var hipLength = Vector3.Distance(leftLeg.Hip.position, leftLeg.Ankle.position);
            var ankleLength = Vector3.Distance(leftLeg.Ankle.position, leftLeg.Foot.position);
            var hipsDistance = Vector3.Distance(leftLeg.Hip.position, rightLeg.Hip.position);

            var maxStepLength = math.sin(stepSettings.MinStepChassisHeight * math.PI * 0.5f) * ankleLength;

            return new ChassisSettings()
            {
                HipLength = hipLength,
                AnkleLength = ankleLength,
                HipsDistance = hipsDistance,

                MaxStepLength = maxStepLength,
                LegLength = ankleLength + hipsDistance,
                StepLength = maxStepLength * stepSettings.StepLengthCf,
                MaxHeightDelta = ankleLength,

                // note: smooth, but can be too slow for small fast mechs
                ChassisRotationSpeed = stepSettings.MaxSteerAngle / (stepSettings.Duration * 1.5f)
            };
        }
    }

}
