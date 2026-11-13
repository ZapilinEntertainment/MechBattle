namespace ZE.MechBattle.Views
{
    public interface IComplexView : IView
    {
        bool TryGetViewPart<T>(ViewPartKey key, out T view) where T : IView;
    
    }
}
