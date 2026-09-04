using Scellecs.Morpeh;

namespace ZE.MechBattle
{
    public class LocalPlayerMechControlsSetFlag : IFlag
    {
        public readonly Entity PlayerEntity;
        public readonly Entity VehicleEntity;

        public LocalPlayerMechControlsSetFlag(Entity playerEntity, Entity vehicleEntity)
        {
            PlayerEntity = playerEntity;
            VehicleEntity = vehicleEntity;
        }

    }
}
