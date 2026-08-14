using Scellecs.Morpeh;
using VContainer;
using Unity.Mathematics;
using ZE.MechBattle.Ecs;

namespace ZE.MechBattle
{
    public class MechFactory : IEntityCreationFactory
    {
        private readonly MechChassisFactory _chassisFactory;
        private readonly MonoViewFactory _viewFactory;
        private readonly TransformAspectHandler _transformAspectHandler;
        private readonly ParentingRelationsApplier _parentingRelationsApplier;
        private readonly MechConfig _mechConfig;

        private readonly Stash<MechComponent> _mechComponents;
        private readonly Stash<RotationSpeedComponent> _rotationSpeed;
        

        [Inject]
        public MechFactory(
            MonoViewFactory viewFactory, 
            TransformAspectHandler transformAspectHandler, 
            MechChassisFactory chassisFactory,
            World world,
            ParentingRelationsApplier parentingRelationsApplier,
            [Key(DevelopConstants.DEFAULT_MECH_ID)] MechConfig mechConfig)
        {
            _viewFactory = viewFactory;
            _transformAspectHandler = transformAspectHandler;
            _chassisFactory = chassisFactory;
            _parentingRelationsApplier = parentingRelationsApplier;
            _mechConfig = mechConfig;

            _mechComponents = world.GetStash<MechComponent>();
            _rotationSpeed = world.GetStash<RotationSpeedComponent>();
        }

        public Entity Build(float3 position, quaternion rotation)
        {
            var mechEntity = _viewFactory.CreateViewReceiver(DevelopConstants.DEFAULT_MECH_ID + "_view");
            _transformAspectHandler.MoveToPoint(mechEntity, position, rotation);

            var chassisEntity = _chassisFactory.Build(mechEntity);
            var upperPartEntity = BuildUpperPart(chassisEntity);

            _mechComponents.Add(mechEntity, new(chassisEntity, upperPartEntity));            

            return mechEntity;
        }

        private Entity BuildUpperPart(Entity parent)
        {
            var upperPartEntity = _parentingRelationsApplier.CreateChildEntityForViewPart(
               new(quaternion.identity, float3.zero),
               parent,
               new(ViewPartType.UpperPart));

            _rotationSpeed.Set(upperPartEntity, new(_mechConfig.UpperPartRotationSpeedRadians));
            return upperPartEntity;
        }
    }
}
