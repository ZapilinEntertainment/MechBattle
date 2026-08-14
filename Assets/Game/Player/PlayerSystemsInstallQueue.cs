using UnityEngine;
using ZE.MechBattle.Ecs;

namespace ZE.MechBattle
{
    public class PlayerSystemsInstallQueue : FeatureSystemsInstallQueue
    {
        protected override void Configure(ISystemsOperator installer)
        {
            installer.AddSystem<PlayerInputSystem>(SystemGroupOrder.PlayerInput);
        }
    }
}
