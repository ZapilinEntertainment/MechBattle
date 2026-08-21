using ZE.MechBattle.Ecs;

namespace ZE.MechBattle
{
    public class GridSystemsInstallQueue : FeatureSystemsInstallQueue
    {
        protected override void Configure(ISystemsOperator installer)
        {
            installer.AddSystem<UnitsGridFillSystem>(TriangularPosUpdateSystem.GroupOrder + 1);
        }
    }
}
