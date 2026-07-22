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

        [Inject]
        public MechFactory(MonoViewFactory viewFactory, TransformAspectHandler transformAspectHandler, MechChassisFactory chassisFactory)
        {
            _viewFactory = viewFactory;
            _transformAspectHandler = transformAspectHandler;
            _chassisFactory = chassisFactory;
        }

        public Entity Build(float3 position, quaternion rotation)
        {
            var mechEntity = _viewFactory.CreateViewReceiver(DevelopConstants.DEFAULT_MECH_VIEW_ID);
            _transformAspectHandler.MoveToPoint(mechEntity, position, rotation);
            _chassisFactory.Build(mechEntity);
            return mechEntity;
        }
    }
}
