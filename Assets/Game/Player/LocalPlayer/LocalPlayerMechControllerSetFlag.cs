namespace ZE.MechBattle
{
    public class LocalPlayerMechControllerSetFlag : IFlag
    {
        public readonly MechControlsWorker MechController;

        public LocalPlayerMechControllerSetFlag(MechControlsWorker mechController) => MechController = mechController;
    
    }
}
