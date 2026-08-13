using Scellecs.Morpeh;

namespace ZE.MechBattle
{
    public class LocalPlayerViewInstancedFlag : IFlag
    {
        public readonly Entity PlayerEntity;
        public readonly Entity VehicleEntity;

        public LocalPlayerViewInstancedFlag(Entity playerEntity, Entity vehicleEntity)
        {
            PlayerEntity = playerEntity;
            VehicleEntity = vehicleEntity;
        }

    }
}
