namespace ZE.MechBattle
{
    // note that is not similar to unity events order. In case of using events, use suitable interfaces: ex. IFixedSystem
    public enum SystemGroupOrder : byte { 
        EarlyUpdate = 0, 
        PlayerInput = 1,
        MechSystemsCalculation,
        MechSystemsApplication,
        Default,     
        SquadUpdates,
        RegularUpdate, 
        Pathfinding,
        UnitsNextPositionCalculation1,
        UnitsNextPositionCalculation2,
        UnitsNextPositionCalculation3,
        UnitsMovement,
        PostMovement,
        WeaponUpdates, 
        TransformUpdates1,
        TransformUpdates2,        
        ViewsLoading,
        DamageCalculation,
        AfterDamageCalculation,
        DamageApply1,
        DamageApply2,
        EnergySystem,
        Repairs,
        DisposeTagsSharing, // share dispose tag with connected (child or linked) objects
        DisposedObjectsOperations,
        Dispose,
        AfterDispose }
}
