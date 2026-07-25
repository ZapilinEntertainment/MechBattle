using TriInspector;
using Unity.Mathematics;
using UnityEngine;

namespace ZE.MechBattle
{
    [CreateAssetMenu(fileName = "MechChassisData", menuName = "Scriptable Objects/MechChassisData")]
    public class MechChassisData : ScriptableObject
    {
        [field:SerializeField] public RigidTransform ChassisRootLocalPoint { get;private set; }
        [field:SerializeField] public float2 FootSize { get; private set; }
        [ShowInInspector, EnableIf(nameof(_unlockEditing))] public LegDataContainer<RigidTransform> LeftLegLocalPoints { get; private set; }
        [ShowInInspector, EnableIf(nameof(_unlockEditing))] public LegDataContainer<RigidTransform> RightLegLocalPoints { get; private set; }
        [ShowInInspector, EnableIf(nameof(_unlockEditing))] public StepSettings StepSettings { get; private set; }
        [ShowInInspector, EnableIf(nameof(_unlockEditing))] public float3 LeftFootDefaultLocalPos { get; private set; }
        [ShowInInspector, EnableIf(nameof(_unlockEditing))] public float3 RightFootDefaultLocalPos { get; private set; }
        [ShowInInspector, EnableIf(nameof(_unlockEditing))] public ChassisSettings ChassisSettings { get; private set; }
        [SerializeField] private bool _unlockEditing = false;

        public bool TryUpdateData(MechView mechView, StepSettings stepSettings)
        {
            if (!CheckViewParts(mechView))
                return false;


            // we need local matrix of chassis in root transform space (ignoring parents between them)
            var chassisRootTransform = mechView.ChassisRoot;
            ChassisRootLocalPoint = MathExtensions.CalculateLocalPointInPointSpace(mechView.Transform, chassisRootTransform);
            LeftLegLocalPoints = WriteLegData(mechView.LeftLeg);
            RightLegLocalPoints = WriteLegData(mechView.RightLeg);

            LeftFootDefaultLocalPos = chassisRootTransform.InverseTransformPoint(mechView.LeftLeg.Foot.position);
            RightFootDefaultLocalPos = chassisRootTransform.InverseTransformPoint(mechView.RightLeg.Foot.position);

            StepSettings = stepSettings;
            ChassisSettings = CalculateChassisSettingsCommand.Execute(mechView.LeftLeg, mechView.RightLeg, StepSettings);

            return true;

            LegDataContainer<RigidTransform> WriteLegData(LegDataContainer<Transform> legData) =>
                new()
                {
                    Hip = GetLocalPoint(legData.Hip),
                    Ankle = GetLocalPoint(legData.Ankle),
                    Foot = GetLocalPoint(legData.Foot),
                };

            RigidTransform GetLocalPoint(Transform transform) => new(transform.localRotation, transform.localPosition);
        }


        private bool CheckViewParts(MechView mechView)
        {
            if (mechView.ChassisRoot == null)
            {
                UnityEngine.Debug.LogError("no chassis root set");
                return false;
            }

            if (!CheckLeg(mechView.LeftLeg, false)) return false;
            if (!CheckLeg(mechView.RightLeg, false)) return false;

            return true;
        }

        private bool CheckLeg(LegDataContainer<Transform> legContainer, bool isRightSide)
        {
            if (legContainer.Hip == null)
            {
                LogAbsenceString("hip");
                return false;
            }

            if (legContainer.Ankle == null)
            {
                LogAbsenceString("ankle");
                return false;
            }

            if (legContainer.Foot == null)
            {
                LogAbsenceString("foot");
                return false;
            }

            return true;

            void LogAbsenceString(string partKey) => UnityEngine.Debug.LogError($"no {(isRightSide ? "right" : "left")} {partKey} set");
        }

    }
}
