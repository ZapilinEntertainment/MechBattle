namespace ZE.MechBattle
{
    public interface IDamageableView : IMonoView
    {
        string ViewDestroyEffectKey { get; }
        DamageableEntityParameters GetParameters();

        /// <summary>
        /// Please do not add more than 10 colliders on same IDamageable. 
        /// Otherwise, fix CollidersTable expected constant
        /// </summary>
        /// <returns></returns>
        int[] GetColliderIds();
        
    
    }
}
