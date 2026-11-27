using Scellecs.Morpeh;
using ZE.MechBattle.Ecs;

namespace ZE.MechBattle
{
    public class HexVisibilityComponentApplier : HexCreationComponentApplier<HexSectorsVisibilityMaskComponent>
    {
        public HexVisibilityComponentApplier(World world) : base(world)
        {
        }

        protected override HexSectorsVisibilityMaskComponent InitialValue => new();
    }
}
