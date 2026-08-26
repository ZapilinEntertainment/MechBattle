using Scellecs.Morpeh;
using VContainer;
using Unity.Mathematics;
using ZE.MechBattle.Ecs;

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
    
        public ChassisEntities Build(Entity mechEntity)
        {
            var chassisRootEntity = _parentingRelationsApplier.CreateChildEntityForViewPart(_mechChassisData.ChassisRootLocalPoint, mechEntity, new(ViewPartType.ChassisRoot));

            var leftLegContainer = CreateLeg(_mechChassisData, chassisRootEntity, isRight: false);
            var rightLegContainer = CreateLeg(_mechChassisData, chassisRootEntity, isRight: true);
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

        private LegDataContainer<Entity> CreateLeg(MechChassisData chassisData, Entity chassisRootEntity, bool isRight)
        {
            var index = isRight ? 1 : 0;
            var legData = isRight ? chassisData.RightLegLocalPoints : chassisData.LeftLegLocalPoints;
            var hipEntity = _parentingRelationsApplier.CreateChildEntityForViewPart(legData.Hip, chassisRootEntity, new (ViewPartType.Hip, index));
            var ankleEntity = _parentingRelationsApplier.CreateChildEntityForViewPart(legData.Ankle, hipEntity, new (ViewPartType.Ankle, index));
            var footEntity = _parentingRelationsApplier.CreateChildEntityForViewPart(legData.Foot, ankleEntity, new(ViewPartType.Foot, index));

            return new()
            {
                Hip = hipEntity,
                Ankle = ankleEntity,
                Foot = footEntity
            };
        }
    }
}
