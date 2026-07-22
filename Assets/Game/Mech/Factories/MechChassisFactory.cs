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
        private readonly Stash<ViewPartRequestComponent> _viewPartsRequestComponent;

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
            _viewPartsRequestComponent = _world.GetStash<ViewPartRequestComponent>();
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
            _viewPartsRequestComponent.Add(chassisRootEntity, new(ViewPartType.ChassisRoot));

            _chassisComponents.Set(mechEntity, new()
            {
                ChassisRootEntity = chassisRootEntity,
                LeftLeg = CreateLeg(_mechChassisData.LeftLegLocalPoints, chassisRootEntity, isRight: false),
                RightLeg = CreateLeg(_mechChassisData.RightLegLocalPoints, chassisRootEntity, isRight: true),
            });
        }

        private LegDataContainer<Entity> CreateLeg(LegDataContainer<RigidTransform> legData, Entity chassisRootEntity, bool isRight)
        {
            var hipEntity = CreateLegPartEntity(legData.Hip, chassisRootEntity);
            var ankleEntity = CreateLegPartEntity(legData.Ankle, hipEntity);
            var footEntity = CreateLegPartEntity(legData.Foot, ankleEntity);

            var index = isRight ? 1 : 0;
            _viewPartsRequestComponent.Add(hipEntity, new(ViewPartType.Hip, index));
            _viewPartsRequestComponent.Add(ankleEntity, new(ViewPartType.Ankle, index));
            _viewPartsRequestComponent.Add(footEntity, new(ViewPartType.Foot, index));

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
