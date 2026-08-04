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
        private readonly Stash<MechComponent> _mechComponents;

        [Inject]
        public MechFactory(
            MonoViewFactory viewFactory, 
            TransformAspectHandler transformAspectHandler, 
            MechChassisFactory chassisFactory,
            World world)
        {
            _viewFactory = viewFactory;
            _transformAspectHandler = transformAspectHandler;
            _chassisFactory = chassisFactory;

            _mechComponents = world.GetStash<MechComponent>();
        }

        public Entity Build(float3 position, quaternion rotation)
        {
            var mechEntity = _viewFactory.CreateViewReceiver(DevelopConstants.DEFAULT_MECH_VIEW_ID);
            _transformAspectHandler.MoveToPoint(mechEntity, position, rotation);
            var chassisEntity = _chassisFactory.Build(mechEntity);
            _mechComponents.Add(mechEntity, new(chassisEntity));
            return mechEntity;
        }
    }
}
