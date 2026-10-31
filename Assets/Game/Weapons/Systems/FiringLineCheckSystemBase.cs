using Scellecs.Morpeh;
using Unity.Collections;
using Unity.IL2CPP.CompilerServices;
using Unity.Jobs;
using UnityEngine;

namespace ZE.MechBattle.Ecs {
    [Il2CppSetOption(Option.NullChecks, false)]
    [Il2CppSetOption(Option.ArrayBoundsChecks, false)]
    [Il2CppSetOption(Option.DivideByZeroChecks, false)]
    public abstract class FiringLineCheckSystemBase<T> : PausableSystem
        where T : unmanaged, IWeaponAimTagComponent
    {
        protected bool _isJobActive = false;
        protected JobHandle _activeJobHandle;
        protected NativeList<RaycastCommand> _commandsList;
        protected NativeList<RaycastHit> _resultsList;

        protected Stash<AttackTargetComponent> _attackTargets;
        protected Stash<FireLineClearTag> _fireLineClearTag;
        protected Stash<WeaponShotPoint> _weaponShotPoints;

        protected Filter _filter;       

        public FiringLineCheckSystemBase(SceneFlagsManager flags) : base(flags)
        {
        }

        public override void OnAwake()
        {
            _filter = World.Filter
               .With<AttackTargetComponent>()
               .With<T>()
               .With<AttackRangeReachedTag>()
               .Build();

            _attackTargets = World.GetStash<AttackTargetComponent>();
            _fireLineClearTag = World.GetStash<FireLineClearTag>();
            _weaponShotPoints = World.GetStash<WeaponShotPoint>();

            _commandsList = new NativeList<RaycastCommand>(Allocator.Persistent);
            _resultsList = new NativeList<RaycastHit>(Allocator.Persistent);
        }

        public override void OnUpdate(float deltaTime)
        {
            if (_isJobActive)
            {
                ApplyPreviousJob();
                _isJobActive = false;
            }

            if (IsPaused || _filter.IsEmpty())
                return;

            LaunchNextJob();
        }

        protected override void InternalDispose()
        {
            _activeJobHandle.Complete();
            _commandsList.Dispose();
            _resultsList.Dispose();
        }

        protected abstract void ApplyPreviousJob();
        protected abstract void LaunchNextJob();
    }
}