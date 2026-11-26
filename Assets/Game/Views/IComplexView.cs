namespace ZE.MechBattle
{
    public interface IComplexView
    {
        bool TryGetPartView(ViewPartKey key, out IView partView);


    }
}
