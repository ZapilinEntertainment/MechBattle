using R3;
using Scellecs.Morpeh;
using System;
using VContainer;
using UnityEngine;
using ZE.MechBattle.Ecs;
using Unity.Mathematics;
using ZE.Workers;

namespace ZE.MechBattle
{
    // todo: rework to system
    public class MechHeadRotationWorker : Worker
    {
        private readonly World _world;
        private readonly MechHandler _mechHandler;
        private readonly SceneFlagsManager _sceneFlags;
        private readonly Stash<LocalRotationLimitComponent> _rotationLimits;
        private readonly Stash<LocalTargetRotationComponent> _rotationTargets;

        private bool _headEntitySet = false;
        private Entity _headEntity;

        [Inject]
        public MechHeadRotationWorker(
            SceneFlagsManager sceneFlags, 
            MechHandler mechHandler,
            World world)
        {
            _mechHandler = mechHandler;
            _sceneFlags = sceneFlags;
            _world = world;

            _rotationLimits = world.GetStash<LocalRotationLimitComponent>();
            _rotationTargets = world.GetStash<LocalTargetRotationComponent>();

           
        }

        public override void Start()
        {
            base.Start();
            _sceneFlags
               .Subscribe<PlayerCameraSetFlag>(OnPlayerCameraSet)
               .AddTo(CompositeDisposable);

            Observable.EveryUpdate()
                .Where(_ => _headEntitySet)
                .Subscribe(Update)
                .AddTo(CompositeDisposable);
        }

        private void OnPlayerCameraSet(PlayerCameraSetFlag flag)
        {
            _headEntity = _mechHandler.GetHeadEntity(flag.VehicleEntity);
            _headEntitySet = true;
        }

        private void Update(Unit unit)
        {
            if (_world.IsDisposed(_headEntity))
                return;

            var cursorPos = Input.mousePosition;
            var x = math.clamp( cursorPos.x / Screen.width, 0f, 1f);
            var y = 1f - math.clamp( cursorPos.y / Screen.height, 0f, 1f);

            SetHeadTarget(_headEntity, new float2(y,x));
        }

        private void SetHeadTarget(Entity headEntity, float2 normalizedInput)
        {
            var limits = _rotationLimits.Get(headEntity).DotLimits.GetDotLimits();
            var rotation = MathExtensions.GetLimitedNormalizedRotation(normalizedInput, limits);

            _rotationTargets.Set(headEntity, new() { Value = rotation });
        }
    }
}
