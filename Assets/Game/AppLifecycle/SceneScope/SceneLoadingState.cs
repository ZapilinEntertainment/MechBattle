using VContainer;
using R3;

namespace ZE.MechBattle.GameStates
{
    public class SceneLoadingState : GameState<SceneStateKey>
    {
        private ReactiveProperty<bool> _isActiveProperty;

        [Inject]
        public SceneLoadingState(SceneFlagsManager flags)
        {
            _isActiveProperty = new ReactiveProperty<bool>(false).AddTo(CompositeDisposable);

            _isActiveProperty
                .CombineLatest(
                    flags.Subscribe<LocalPlayerMechControllerSetFlag>(),
                    (a, b) => a & b)
                .Where(x => x == true)
                .Subscribe(_ => OnPlayerMechControlsStart())
                .AddTo(CompositeDisposable);
            
        }

        public override void OnEnter() 
        {
            _isActiveProperty.Value = true;
        }

        public override void OnExit() 
        {
            _isActiveProperty.Value = false;
        }

        private void OnPlayerMechControlsStart()
        {
            SwitchState(SceneStateKey.Game);
#if UNITY_EDITOR
            UnityEngine.Debug.Log("mech controls set, game starting");
#endif
        }
    }
}
