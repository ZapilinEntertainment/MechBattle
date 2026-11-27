using Scellecs.Morpeh;
using ZE.MechBattle.Ecs;

namespace ZE.MechBattle
{
    public class HexCaptureDataApplier : HexCreationComponentApplier<HexCaptureStateComponent>
    {
        public HexCaptureDataApplier(World world) : base(world)
        {
        }

        protected override HexCaptureStateComponent InitialValue => HexCaptureStateComponent.Empty;
    }
}
