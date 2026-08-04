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


        [Inject]
        public MechMovementHandler(World world, TransformAspectHandler transformAspectHandler)
        {
            _parentComponents = world.GetStash<ParentEntityComponent>();
            _chassisComponents = world.GetStash<MechChassisComponent>();
            _activeLegs = world.GetStash<MechActiveLegValueComponent>();
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

    }
}
