using R3;
using Scellecs.Morpeh;
using VContainer;
using ZE.MechBattle.UI;

namespace ZE.MechBattle
{
    public class PlayerUiInitializer
    {
        private readonly LifetimeTrackingManager _lifetimeTrackingManager;
        private readonly PlayerInterfaceWorker _interfaceWorker;
        private readonly CursorAimTrackingWorker _aimWorker;
        private Entity _playerEntity;

        [Inject]
        public PlayerUiInitializer(LifetimeTrackingManager lifetimeTrackingManager)
        {
            _lifetimeTrackingManager = lifetimeTrackingManager;
        }
    
        public void Activate(Entity playerEntity, Entity vehicleEntity)
        {
            _playerEntity = playerEntity;
            SetupPlayerMechWeaponsAim(vehicleEntity);
            
        }

        private void SetupPlayerMechWeaponsAim(Entity vehicleEntity)
        {
            //var lifetimeObject = _lifetimeTrackingManager.GetEntityLifetimeObject(_playerEntity);

        }
    }
}
