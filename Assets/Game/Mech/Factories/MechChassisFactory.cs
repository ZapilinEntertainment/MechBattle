using Scellecs.Morpeh;
using VContainer;
using Unity.Mathematics;
using ZE.MechBattle.Ecs;

namespace ZE.MechBattle
{
    public class MechChassisFactory
    {
        private readonly World _world;
        private readonly ParentingRelationsApplier _parentingRelationsApplier;
        private readonly MechChassisData _mechChassisData;

        private readonly Stash<MechChassisComponent> _chassisComponents;
        private readonly Stash<ViewPartRequestComponent> _viewPartsRequestComponents;
        private readonly Stash<ChassisSettingsComponent> _stepSettingsComponents;
        private readonly Stash<InitialLocalPosition> _initLocalPositions;

        [Inject]
        public MechChassisFactory(
            World world, 
            ParentingRelationsApplier parentingRelationsApplier,
            [Key(DevelopConstants.DEFAULT_MECH_VIEW_ID)] MechChassisData mechChassisData)
        {
            _world = world;
            
            _mechChassisData = mechChassisData;
            _parentingRelationsApplier = parentingRelationsApplier;

            _chassisComponents = _world.GetStash<MechChassisComponent>();
            _viewPartsRequestComponents = _world.GetStash<ViewPartRequestComponent>();
            _stepSettingsComponents = _world.GetStash<ChassisSettingsComponent>();
            _initLocalPositions = _world.GetStash<InitialLocalPosition>();
        }
    
        public void Build(Entity mechEntity)
        {
            var chassisRootEntity = _world.CreateEntity();
            _parentingRelationsApplier.Apply(new()
            {
                ChildEntity = chassisRootEntity,
                ParentEntity= mechEntity,
                LocalPos = _mechChassisData.ChassisRootLocalPoint.pos,
                LocalRot = _mechChassisData.ChassisRootLocalPoint.rot,
                AwaitParentViewComponent = true
            });
            _viewPartsRequestComponents.Add(chassisRootEntity, new(ViewPartType.ChassisRoot));

            var leftLegContainer = CreateLeg(_mechChassisData, chassisRootEntity, isRight: false);
            var rightLegContainer = CreateLeg(_mechChassisData, chassisRootEntity, isRight: true);
            _chassisComponents.Set(mechEntity, new()
            {
                ChassisRootEntity = chassisRootEntity,
                LeftLeg = leftLegContainer,
                RightLeg = rightLegContainer,
            });
            _stepSettingsComponents.Set(chassisRootEntity, new(_mechChassisData.ChassisSettings, _mechChassisData.StepSettings, _mechChassisData.FootSize));

            // save foot default local pos in chassis space
            _initLocalPositions.Set(leftLegContainer.Foot, new(_mechChassisData.LeftFootDefaultLocalPos));
            _initLocalPositions.Set(rightLegContainer.Foot, new(_mechChassisData.RightFootDefaultLocalPos));
        }

        private LegDataContainer<Entity> CreateLeg(MechChassisData chassisData, Entity chassisRootEntity, bool isRight)
        {
            var legData = isRight ? chassisData.LeftLegLocalPoints : chassisData.RightLegLocalPoints;
            var hipEntity = CreateLegPartEntity(legData.Hip, chassisRootEntity);
            var ankleEntity = CreateLegPartEntity(legData.Ankle, hipEntity);
            var footEntity = CreateLegPartEntity(legData.Foot, ankleEntity);

            var index = isRight ? 1 : 0;
            _viewPartsRequestComponents.Add(hipEntity, new(ViewPartType.Hip, index));
            _viewPartsRequestComponents.Add(ankleEntity, new(ViewPartType.Ankle, index));
            _viewPartsRequestComponents.Add(footEntity, new(ViewPartType.Foot, index));

            return new()
            {
                Hip = hipEntity,
                Ankle = ankleEntity,
                Foot = footEntity
            };
        }

        private Entity CreateLegPartEntity(RigidTransform point, Entity parent)
        {
            var entity = _world.CreateEntity();
            _parentingRelationsApplier.Apply(new()
            {
                ParentEntity = parent,
                ChildEntity = entity,
                AwaitParentViewComponent= true,
                LocalPos = point.pos,
                LocalRot = point.rot,
            });
            return entity;
        }
    }
}
