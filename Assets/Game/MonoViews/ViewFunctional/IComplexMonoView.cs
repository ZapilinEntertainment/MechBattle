namespace ZE.MechBattle
{
    public interface IComplexMonoView : IMonoView
    {
        bool TryGetPartByKey(ViewPartKey key, out IViewPart viewPart);
    }
}
