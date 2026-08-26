using Scellecs.Morpeh;
using VContainer;
using ZE.MechBattle.Ecs;
using System.Collections.Generic;

namespace ZE.MechBattle.MechBuilding
{
    public class MechChassisFactory
    {
        public struct ChassisEntities
        {
            public Entity ChassisRoot;
            public LegDataContainer<Entity> LeftLeg;
            public LegDataContainer<Entity> RightLeg;
        }

        private Entity _mechEntity;

        private readonly World _world;
        private readonly ParentingRelationsApplier _parentingRelationsApplier;
        private readonly MechChassisData _mechChassisData;

        private readonly Stash<MechChassisComponent> _chassisComponents;
        private readonly Stash<ChassisSettingsComponent> _stepSettingsComponents;
        private readonly Stash<InitialLocalPosition> _initLocalPositions;
        private readonly Stash<MechActiveLegValueComponent> _activeLegs;

        [Inject]
        public MechChassisFactory(
            World world, 
            ParentingRelationsApplier parentingRelationsApplier,
            [Key(DevelopConstants.DEFAULT_MECH_ID)] MechChassisData mechChassisData)
        {
            _world = world;
            
            _mechChassisData = mechChassisData;
            _parentingRelationsApplier = parentingRelationsApplier;

            _chassisComponents = _world.GetStash<MechChassisComponent>();
            _stepSettingsComponents = _world.GetStash<ChassisSettingsComponent>();
            _initLocalPositions = _world.GetStash<InitialLocalPosition>();
            _activeLegs = _world.GetStash<MechActiveLegValueComponent>();
        }
    
        public ChassisEntities Build(Entity mechEntity, ICollection<ViewPartKey> separatedPartKeys)
        {
            _mechEntity = mechEntity;

            var rootKey = ViewPartKey.Chassis;
            var chassisRootEntity = _parentingRelationsApplier.CreateChildEntityForViewPart(
                _mechChassisData.ChassisRootLocalPoint,
                _mechEntity,
                _mechEntity,
                rootKey,
                separateViewObject: separatedPartKeys.Contains(rootKey));

            var leftLegContainer = CreateLeg(_mechChassisData, chassisRootEntity, isRight: false, separatedPartKeys);
            var rightLegContainer = CreateLeg(_mechChassisData, chassisRootEntity, isRight: true, separatedPartKeys);
            _chassisComponents.Set(chassisRootEntity, new()
            {
                LeftLeg = leftLegContainer,
                RightLeg = rightLegContainer,
            });
            _stepSettingsComponents.Add(chassisRootEntity, new(_mechChassisData.ChassisSettings, _mechChassisData.StepSettings, _mechChassisData.FootSize));

            // save foot default local pos in chassis space
            _initLocalPositions.Add(leftLegContainer.Foot, new(_mechChassisData.LeftFootDefaultLocalPos));
            _initLocalPositions.Add(rightLegContainer.Foot, new(_mechChassisData.RightFootDefaultLocalPos));

            _activeLegs.Add(chassisRootEntity, MechActiveLegValueComponent.Idle);

            //UnityEngine.Debug.Log($"mech entity {mechEntity.Id}, chassis entity {chassisRootEntity.Id}");
            return new()
            {
                ChassisRoot = chassisRootEntity,
                LeftLeg = leftLegContainer,
                RightLeg = rightLegContainer
            };
        }

        private LegDataContainer<Entity> CreateLeg(
            MechChassisData chassisData, 
            Entity chassisRootEntity, 
            bool isRight, 
            ICollection<ViewPartKey> separatedPartKeys)
        {
            var index = isRight ? 1 : 0;
            var legData = isRight ? chassisData.RightLegLocalPoints : chassisData.LeftLegLocalPoints;

            var hipKey = ViewPartKey.GetHipKey(isRight);
            var hipEntity = _parentingRelationsApplier.CreateChildEntityForViewPart(
                legData.Hip, 
                chassisRootEntity, 
                _mechEntity,
                hipKey, 
                separatedPartKeys.Contains(hipKey));

            var ankleKey = ViewPartKey.GetAnkleKey(isRight);
            var ankleEntity = _parentingRelationsApplier.CreateChildEntityForViewPart(
                legData.Ankle, 
                hipEntity, 
                _mechEntity, 
                ankleKey,
                separatedPartKeys.Contains(ankleKey));

            var footKey = ViewPartKey.GetFootKey(isRight);
            var footEntity = _parentingRelationsApplier.CreateChildEntityForViewPart(
                legData.Foot, 
                ankleEntity, 
                _mechEntity,
                footKey,
                separatedPartKeys.Contains(footKey));

            return new()
            {
                Hip = hipEntity,
                Ankle = ankleEntity,
                Foot = footEntity
            };
        }
    }
}
