namespace ZE.MechBattle
{
    public interface IComplexMonoView : IMonoView
    {
        bool TryGetMonoPartView(ViewPartKey key, out IMonoView viewPart);
    }
}
