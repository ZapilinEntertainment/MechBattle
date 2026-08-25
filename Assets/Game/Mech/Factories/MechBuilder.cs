using Scellecs.Morpeh;
using Unity.Mathematics;
using UnityEngine;
using VContainer;
using ZE.MechBattle.Ecs;

namespace ZE.MechBattle
{
    public class MechBuilder
    {
        public Entity MechEntity { get; private set; }
        public Entity ChassisEntity { get; private set; }
        public Entity UpperPartEntity { get; private set; }
        public Entity HeadEntity { get; private set; }

        private readonly MonoViewFactory _viewFactory;
        private readonly TransformAspectHandler _transformAspectHandler;
        private readonly Stash<MechComponent> _mechComponent;

        [Inject]
        public MechBuilder(MonoViewFactory viewFactory, TransformAspectHandler transformAspectHandler, World world)
        {
            _viewFactory = viewFactory;
            _transformAspectHandler = transformAspectHandler;
            _mechComponent = world.GetStash<MechComponent>();
        }

        public Entity Build(MechConfig mechConfig, float3 position, quaternion rotation)
        {
            MechEntity = _viewFactory.CreateViewReceiver(DevelopConstants.DEFAULT_MECH_ID + "_view");
            _transformAspectHandler.MoveToPoint(MechEntity, position, rotation);

            return MechEntity;
        }

        public void CheckCrucialParts(MechPartsBuilder partsBuilder)
        {
            if (!partsBuilder.TryGetConstructedPartEntity(MechConstants.HEAD_PART_ID, out var headEntity))
                UnityEngine.Debug.LogError("head part was not added");
            else
                HeadEntity = headEntity;



            if (!partsBuilder.TryGetConstructedPartEntity(MechConstants.UPPER_PART_ID, out var upperPartEntity))
                UnityEngine.Debug.LogError("upper part was not added");
            else
                UpperPartEntity = upperPartEntity;



            if (!partsBuilder.TryGetConstructedPartEntity(MechConstants.CHASSIS_PART_ID, out var chassisPartEntity))
                UnityEngine.Debug.LogError("chassis was not added");
            else
                ChassisEntity = chassisPartEntity;



            var mechComponent = new MechComponent(ChassisEntity, UpperPartEntity, HeadEntity);
            _mechComponent.Add(MechEntity, mechComponent);
        }
    }
}
