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
            var colors = new Color[2] { Color.red, Color.blue };
            var i = 0;
            foreach (var weaponEntity in _weaponHandler.GetNextWeaponEntity(mechEntity))
            {
                var color = colors[i];
                StartWeaponTracking(weaponEntity, mechEntity, mechEntityLifetimeObject, 
                    new()
                {
                    Color = color,
                    Level = i
                });
                i = (i + 1) % colors.Length;
            }
        }

        private void StartWeaponTracking(Entity trackingWeapon, Entity mechEntity, DisposableBag lifetimeObject, WeaponAimMarkerDisplayProtocol displayProtocol)
        {
            var markerWorker = _resolver.Resolve<WeaponAimMarkerWorker>();
            lifetimeObject.Add(markerWorker);
            markerWorker.Start(trackingWeapon, mechEntity, displayProtocol);
        }
    }
}
