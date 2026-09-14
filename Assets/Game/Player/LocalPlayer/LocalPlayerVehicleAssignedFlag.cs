using Scellecs.Morpeh;

namespace ZE.MechBattle
{
    public class LocalPlayerVehicleAssignedFlag : IFlag
    {
        public readonly Entity PlayerEntity;
        public readonly Entity VehicleEntity;

        public LocalPlayerVehicleAssignedFlag(Entity playerEntity, Entity vehicleEntity)
        {
            PlayerEntity = playerEntity;
            VehicleEntity = vehicleEntity;
        }

    }
}
