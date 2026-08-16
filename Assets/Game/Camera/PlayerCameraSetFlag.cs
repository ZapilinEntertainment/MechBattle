using Scellecs.Morpeh;

namespace ZE.MechBattle
{
    public class PlayerCameraSetFlag : IFlag
    {
        public readonly Entity VehicleEntity;
        public PlayerCameraSetFlag(Entity vehicleEntity)
        {
            VehicleEntity = vehicleEntity;
        }
    
    }
}
