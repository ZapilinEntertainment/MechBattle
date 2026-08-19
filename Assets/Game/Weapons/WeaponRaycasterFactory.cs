using Scellecs.Morpeh;
using VContainer;
using ZE.MechBattle.Ecs;

namespace ZE.MechBattle
{
    public class WeaponRaycasterFactory
    {
        private readonly World _world;
        private readonly RayEffectFactory _rayEffectFactory;
        private readonly WeaponHandler _weaponHandler;
        private readonly Stash<WeaponRayComponent> _weaponRayComponents;

        [Inject]
        public WeaponRaycasterFactory(World world, RayEffectFactory rayEffectFactory, WeaponHandler weaponHandler)
        {
            _world = world;
            _rayEffectFactory = rayEffectFactory;
            _weaponHandler = weaponHandler;

            _weaponRayComponents = _world.GetStash<WeaponRayComponent>();
        }

        public IWeaponRayCaster Create(Entity weaponEntity)
        {
            var rayComponent = _weaponRayComponents.Get(weaponEntity);
            var effectView = _rayEffectFactory.Create(rayComponent.Id);
            var maxRange = _weaponHandler.GetWeaponMaxRange(weaponEntity);
            return new WeaponRaycaster(_world, weaponEntity, effectView, maxRange);
        }
    
    }
}
