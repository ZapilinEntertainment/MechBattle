using ZE.UiService;
using ZE.Workers;
using R3;
using System;
using System.Collections.Generic;
using VContainer;

namespace ZE.MechBattle
{
    public interface IMechUIElementsVisibilityController
    {
        Observable<MechInterfaceElementsVisibilityFlags> VisibilityFlagsProperty { get; }
    }
}

namespace ZE.MechBattle.Mech.UI
{
    public class UIMechInterfaceWorker : Worker, IMechUIElementsVisibilityController
    {
        [Flags]
        private enum MechInterfaceSituations
        {
            Default = 0,
            LaserEyesActive = 1 << 0
        }

        public Observable<MechInterfaceElementsVisibilityFlags> VisibilityFlagsProperty => _visibility;

        private readonly ReactiveProperty<MechInterfaceSituations> _currentSituationsProperty;
        private readonly SceneFlagsManager _sceneFlags;
        private ReactiveProperty<MechInterfaceElementsVisibilityFlags> _visibility;

        [Inject]
        public UIMechInterfaceWorker(WindowsManager windowsManager, SceneFlagsManager sceneFlags)
        {
            _currentSituationsProperty = new ReactiveProperty<MechInterfaceSituations>(MechInterfaceSituations.Default).AddTo(CompositeDisposable);
            _visibility = new ReactiveProperty<MechInterfaceElementsVisibilityFlags>(MechInterfaceElementsVisibilityFlags.None).AddTo(CompositeDisposable);

            _sceneFlags = sceneFlags;
            _sceneFlags
                .Subscribe<LocalPlayerMechControllerSetFlag>(OnMechControllerSet)
                .AddTo(CompositeDisposable);            

            _currentSituationsProperty
                .ThrottleLastFrame(1)
                .Subscribe(UpdateVisibilityBaseOnSituation)
                .AddTo(CompositeDisposable);
        }

        private void OnMechControllerSet(LocalPlayerMechControllerSetFlag flag)
        {
            var mechController = flag.MechController;
            mechController.LaserEyesActiveProperty
                .Subscribe(x => ChangeSituationFlag(x, MechInterfaceSituations.LaserEyesActive))
                .AddTo(CompositeDisposable);
        }

        private void ChangeSituationFlag(bool x, MechInterfaceSituations situation)
        {
            if (x)
                _currentSituationsProperty.Value |= situation;
            else
                _currentSituationsProperty.Value &= ~situation;
        }

        private void UpdateVisibilityBaseOnSituation(MechInterfaceSituations situation)
        {
            var visibility = MechInterfaceElementsVisibilityFlags.None;
            if (situation.HasFlag(MechInterfaceSituations.LaserEyesActive))
                visibility |= MechInterfaceElementsVisibilityFlags.LaserEyesAim;
            else
                visibility |= MechInterfaceElementsVisibilityFlags.MainGunAim;

            _visibility.Value = visibility;
        }
    }
}
