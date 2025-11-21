using VContainer;
using R3;

namespace ZE.MechBattle
{
    public class PlayerFactory
    {
        private readonly MechBuilder _mechBuilder;
        private readonly IObjectResolver _resolver;

        [Inject]
        public PlayerFactory(MechBuilder mechBuilder, IObjectResolver resolver)
        {
            _mechBuilder = mechBuilder;
            _resolver = resolver;
        }

        public Player CreateLocalPlayer()
        {
            var player = new LocalPlayer();

            var mech = _mechBuilder.Build();
            mech.AddTo(player.LifetimeObject);            

            var designator = _resolver.Resolve<AimWorker>();
            designator.AddTo(player.LifetimeObject);          
            designator.Start();

            player.SetDesignator(designator);
            player.SetMech(mech);            

            return player;
        }    
    }
}
