namespace ZE.MechBattle
{
    // note that is not similar to unity events order. In case of using events, use suitable interfaces: ex. IFixedSystem
    public enum SystemGroupOrder : byte { 
        EarlyUpdate = 0, 
        PlayerInput = 1,
        MechSystemsCalculation,
        MechSystemsApplication,
        Default, 
        RegularUpdate, 
        WeaponUpdates, 
        TransformUpdates, 
        PostUpdate, 
        ViewsLoading, 
        DisposeTagsSharing, // share dispose tag with connected (child or linked) objects
        DisposedObjectsOperations,
        Dispose,
        AfterDispose }
}
