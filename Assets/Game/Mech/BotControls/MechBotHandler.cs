using Scellecs.Morpeh;
using VContainer;
using ZE.MechBattle.Ecs;

namespace ZE.MechBattle
{
    public class MechBotHandler
    {
        private readonly StatesApplier _statesApplier;
        private readonly MechHandler _mechHandler;
        private readonly MechWeaponsHandler _weaponsHandler;

        

        [Inject]
        public MechBotHandler(StatesApplier statesApplier, MechHandler mechHandler, MechWeaponsHandler weaponsHandler)
        {
            _statesApplier = statesApplier;
            _mechHandler = mechHandler;
            _weaponsHandler = weaponsHandler;
        }

        public void ApplyBotControls(Entity mechEntity, PlayerKey playerKey)
        {
            _statesApplier.ApplyState(mechEntity, BehaviourKey.MechBotCabin, StateKey.Idle, highPriority: true);
            _statesApplier.ApplyState(_mechHandler.GetChassisEntity(mechEntity), BehaviourKey.MechBotChassis, StateKey.Idle, highPriority: false);

            _mechHandler.AssignMechPlayerAffinity(mechEntity, playerKey);
            _weaponsHandler.SetupWeaponsForBotControl(mechEntity, MechConstants.BOT_MAX_ABERRATION);
        }
    
    }
}
