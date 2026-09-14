using R3;
using Scellecs.Morpeh;
using VContainer;
using ZE.MechBattle.Ecs;

namespace ZE.MechBattle.States
{
    public class SceneGameState : GameState<SceneStateKey>
    {
        private ReactiveProperty<bool> _isActiveProperty;
        private Entity _localPlayerMechReactor;
        private Entity _localPlayerMechEntity;

        private readonly World _world;
        private readonly SceneFlagsManager _sceneFlags;
        private readonly Stash<EnergySourceComponent> _energySources;
        private readonly Stash<EntityDisposeTag> _entityDisposedTag;

        [Inject]
        public SceneGameState(SceneFlagsManager sceneFlagsManager, World world)
        {
            _world = world;
            _sceneFlags = sceneFlagsManager;

            _energySources = _world.GetStash<EnergySourceComponent>();
            _entityDisposedTag = _world.GetStash<EntityDisposeTag>();

            _isActiveProperty = new ReactiveProperty<bool>(false).AddTo(CompositeDisposable);

            Observable.EveryUpdate()
                .CombineLatest(_isActiveProperty, (Unit unit, bool isActive) => isActive)
                .Where(x => x == true)
                .Subscribe(_ => CheckReactorCondition())
                .AddTo(CompositeDisposable);
        }

        public override void OnEnter() 
        {
            _localPlayerMechEntity = _sceneFlags.GetFirstFlag<LocalPlayerVehicleAssignedFlag>().VehicleEntity;
            _localPlayerMechReactor = _energySources.Get(_localPlayerMechEntity).SourceEntity;

            _isActiveProperty.Value = true;
        }

        public override void OnExit() 
        {
            _isActiveProperty.Value = false;
        }

        private void CheckReactorCondition()
        {
            if (_world.IsDisposed(_localPlayerMechReactor) || _entityDisposedTag.Has(_localPlayerMechReactor))
            {
                UnityEngine.Debug.Log("mech reactor destroyed, scene failed");
                _entityDisposedTag.Set(_localPlayerMechEntity);
                SwitchState(SceneStateKey.Fail);
            }
        }
    }
}
