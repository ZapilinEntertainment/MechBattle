namespace ZE.MechBattle
{
    // this can be both pool, multidrawer or other realization methods (just like vfx players)
    public interface IRayEffectPlayer
    {
        public IDisposableRayEffectView GetRayEffect();
    
    }
}
