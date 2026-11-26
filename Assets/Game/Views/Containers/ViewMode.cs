namespace ZE.MechBattle
{
    public enum ViewMode : byte
    {
        None,
        SceneMono, // loads with scene
        DelayedMono, // loads into containers
        Virtual, // single virtual view on entity
        ComplexVirtual // multiple virtual views each one on different entity
    
    }
}
