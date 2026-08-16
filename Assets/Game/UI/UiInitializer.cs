using R3;
using Scellecs.Morpeh;
using System;
using UnityEngine;
using VContainer;
using ZE.MechBattle.Ecs;
using ZE.UiService;

namespace ZE.MechBattle
{
    public class UiInitializer : IDisposable
    {
        private IDisposable _flagSubscription;
        private readonly LifetimeTrackingManager _lifetimeTrackingManager;
        private readonly IObjectResolver _resolver;
        private readonly WindowsManager _windowsManager;
        private readonly WeaponHandler _weaponHandler;

        [Inject]
        public UiInitializer(
            SceneFlagsManager flags,  
            LifetimeTrackingManager lifetimeTrackingManager, 
            IObjectResolver resolver, 
            WindowsManager windowsManager,
            WeaponHandler weaponHandler)
        {
            _lifetimeTrackingManager = lifetimeTrackingManager;
            _resolver = resolver;
            _windowsManager = windowsManager;
            _weaponHandler = weaponHandler;

            _flagSubscription = flags.Subscribe<PlayerCameraSetFlag>(OnPlayerCameraSet);
        }

        public void Dispose()
        {
            _flagSubscription.Dispose();
        }

        private void OnPlayerCameraSet(PlayerCameraSetFlag flag)
        {
            _windowsManager.ShowWindow<UIAimWindow>();

            var mechEntity = flag.VehicleEntity;
            var mechEntityLifetimeObject = _lifetimeTrackingManager.GetEntityLifetimeObject(mechEntity);
            foreach (var weaponEntity in _weaponHandler.GetNextWeaponEntity(mechEntity))
            {
                StartWeaponTracking(_weaponHandler.GetWeaponsAimingEntity(weaponEntity), mechEntityLifetimeObject);
            }
        }

        private void StartWeaponTracking(Entity trackingWeapon, DisposableBag lifetimeObject)
        {
            var markerWorker = _resolver.Resolve<WeaponAimMarkerWorker>();
            lifetimeObject.Add(markerWorker);
            markerWorker.Start(trackingWeapon);
        }
    }
}
