using Scellecs.Morpeh;
using Unity.IL2CPP.CompilerServices;

namespace ZE.MechBattle.Ecs 
{
    public interface IWeaponShotCompletenessHandler
    {
        void OnWeaponShot(Entity entity);
    }

    [Il2CppSetOption(Option.NullChecks, false)]
    [Il2CppSetOption(Option.ArrayBoundsChecks, false)]
    [Il2CppSetOption(Option.DivideByZeroChecks, false)]
    public sealed class WeaponLoadingCheckSystem : IntervalUpdateSystemBase<WeaponUpdateComponent>, IWeaponShotCompletenessHandler
    {
        private Stash<GunLoadedTag> _readyToShotTag;

        public WeaponLoadingCheckSystem(SceneFlagsManager flags) : base(flags)
        {
        }

        public override void OnAwake()
        {
            base.OnAwake();
            _readyToShotTag = World.GetStash<GunLoadedTag>();
        }

        public void OnWeaponShot(Entity entity)
        {
            _readyToShotTag.Remove(entity);
            RestartTimer(entity);
        }

        protected override void IntervalUpdate(Entity entity)
        {
            _readyToShotTag.Set(entity);
        }

        protected override FilterBuilder PrepareFilter() =>
            base.PrepareFilter()
            .Without<GunLoadedTag>();
    }
}