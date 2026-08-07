using VContainer;
using Scellecs.Morpeh;
using ZE.MechBattle.Ecs;
using Unity.Mathematics;

namespace ZE.MechBattle.MechMovement
{
    public class MechMovementHandler
    {
        private readonly Stash<MechChassisComponent> _chassisComponents;
        private readonly Stash<ParentEntityComponent> _parentComponents;
        private readonly Stash<MechActiveLegValueComponent> _activeLegs;        

        private readonly Stash<StepProgressionComponent> _stepProgression;
        private readonly Stash<StepInitialPointsPreparedTag> _stepInitialPointsPrepared;
        private readonly Stash<MechInputComponent> _input;

        private readonly Stash<CheckIdlePosTag> _checkIdlePosTags;
        private readonly Stash<ReturnToIdlePosTag> _returnToIdleTags;

        private readonly Stash<ChassisSettingsComponent> _settings;

        private readonly TransformAspectHandler _transformAspectHandler;


        [Inject]
        public MechMovementHandler(World world, TransformAspectHandler transformAspectHandler)
        {
            _transformAspectHandler = transformAspectHandler;

            _parentComponents = world.GetStash<ParentEntityComponent>();
            _chassisComponents = world.GetStash<MechChassisComponent>();
            _activeLegs = world.GetStash<MechActiveLegValueComponent>();

            _stepProgression = world.GetStash<StepProgressionComponent>();
            _stepInitialPointsPrepared = world.GetStash<StepInitialPointsPreparedTag>();
            _input = world.GetStash<MechInputComponent>();

            _checkIdlePosTags = world.GetStash<CheckIdlePosTag>();
            _returnToIdleTags = world.GetStash<ReturnToIdlePosTag>();

            _settings = world.GetStash<ChassisSettingsComponent>();
        }

        public Entity GetMechEntity(Entity chassisEntity) => _parentComponents.Get(chassisEntity).Value;

        public (Entity activeFoot, Entity backFoot) GetFoots(Entity chassisEntity)
        {
            var activeLegIndex = _activeLegs.Get(chassisEntity).Value;
            var component = _chassisComponents.Get(chassisEntity);
            var activeFoot = GetActiveFootEntity(chassisEntity, activeLegIndex);
            var backFoot = component.RightLeg.Foot == activeFoot ? component.LeftLeg.Foot : component.RightLeg.Foot;
            return (activeFoot, backFoot);
        }

        public Entity GetActiveFootEntity(Entity chassisEntity)
        {
            var activeLegIndex = _activeLegs.Get(chassisEntity).Value;
            var component = _chassisComponents.Get(chassisEntity);
            return activeLegIndex == 0 ? component.LeftLeg.Foot : component.RightLeg.Foot;
        }

        public Entity GetActiveFootEntity(Entity chassisEntity, int activeLegIndex)
        {
            var component = _chassisComponents.Get(chassisEntity);
            return activeLegIndex == 0 ? component.LeftLeg.Foot : component.RightLeg.Foot;
        }

        public void SwitchActiveFoot(Entity chassisEntity)
        {
            ref var component = ref _activeLegs.Get(chassisEntity);
            if (component.Value == 0)
                component.Value = 1;
            else
                component.Value = 0;
        }

        #region stop movement
        public float CalculateStopInputValue(Entity chassisEntity)
        {
            var (activeFoot, backFoot) = GetFoots(chassisEntity);

            var stepSettings = _settings.Get(chassisEntity).ChassisSettings;
            // note: inverse active and back foot (step was completed before this call and legs was switched)
            var backFootPos = _transformAspectHandler.GetPosition(backFoot);
            var chassisPoint = _transformAspectHandler.GetPoint(chassisEntity);
            var min = MathExtensions.InverseTransformPoint(backFootPos, chassisPoint).z; 
            return min / stepSettings.StepLength;
        }

        public void ClearMovementData(Entity chassisEntity)
        {
            ClearStepData(chassisEntity);
            _input.Remove(chassisEntity);
        }

        public void OnStepCompleted(Entity chassisEntity)
        {
            //UnityEngine.Debug.Log("step completed");
            ClearStepData(chassisEntity);
            SwitchActiveFoot(chassisEntity);
            if (_returnToIdleTags.Has(chassisEntity))
                _returnToIdleTags.Remove(chassisEntity);
            else
                _checkIdlePosTags.Set(chassisEntity);
        }

        public bool IsStandPoseMovementRequired(Entity chassisEntity)
        {
            var (leftFootZ, rightFootZ) = GetFootLocalZ(chassisEntity);
            return math.abs(leftFootZ - rightFootZ) > 1f;
        }

        private void ClearStepData(Entity chassisEntity)
        {
            _stepInitialPointsPrepared.Remove(chassisEntity);
            _stepProgression.Remove(chassisEntity);
        }
        #endregion

       

        private (float leftFootZ, float rightFootZ) GetFootLocalZ(Entity chassisEntity)
        {
            var chassisComponent = _chassisComponents.Get(chassisEntity);
            var leftFootPos = _transformAspectHandler.GetPosition(chassisComponent.LeftLeg.Foot);
            var rightFootPos = _transformAspectHandler.GetPosition(chassisComponent.RightLeg.Foot);
            var chassisPoint = _transformAspectHandler.GetPoint(chassisEntity);

            var leftFootZ = MathExtensions.InverseTransformPoint(leftFootPos, chassisPoint).z;
            var rightFootZ = MathExtensions.InverseTransformPoint(rightFootPos, chassisPoint).z;
            return (leftFootZ, rightFootZ);
        }
    }
}
