using R3;
using Scellecs.Morpeh;
using System;
using VContainer;
using UnityEngine;
using ZE.MechBattle.Ecs;
using Unity.Mathematics;

namespace ZE.MechBattle
{
    public class MechHeadRotationWorker : IDisposable
    {
        private readonly CompositeDisposable _compositeDisposable = new();
        private readonly MechHandler _mechHandler;
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

            _rotationLimits = world.GetStash<LocalRotationLimitComponent>();
            _rotationTargets = world.GetStash<LocalTargetRotationComponent>();

            sceneFlags
                .Subscribe<PlayerCameraSetFlag>(OnPlayerCameraSet)
                .AddTo(_compositeDisposable);

            Observable.EveryUpdate()
                .Where(_ => _headEntitySet)
                .Subscribe(Update)
                .AddTo(_compositeDisposable);
        }

        public void Dispose()
        {
            _compositeDisposable.Dispose();
        }

        private void OnPlayerCameraSet(PlayerCameraSetFlag flag)
        {
            _headEntity = _mechHandler.GetHeadEntity(flag.VehicleEntity);
            _headEntitySet = true;
        }

        private void Update(Unit unit)
        {
            var cursorPos = Input.mousePosition;
            var x = math.clamp( cursorPos.x / Screen.width, 0f, 1f);
            var y = 1f - math.clamp( cursorPos.y / Screen.height, 0f, 1f);

            SetHeadTarget(_headEntity, new float2(y,x));
        }

        private void SetHeadTarget(Entity headEntity, float2 normalizedInput)
        {
            var limits = _rotationLimits.Get(headEntity).DotLimits.GetDotLimits();
            var rotation = MathExtensions.GetLimitedNormalizedRotation(normalizedInput, limits);

            //_rotationTargets.Set(headEntity, new() { Value = rotation });
            ref var component = ref headEntity.GetComponent<LocalRotationComponent>();
            component.Value = rotation;
            headEntity.SetComponent<TransformUpdatedTag>(new());
        }
    }
}
