using ZE.MechBattle.Views;

namespace ZE.MechBattle
{
    public abstract class ComplexMonoView : SimpleView, IComplexView
    {
        abstract public bool TryGetMonoPartView(ViewPartKey key, out IMonoView viewPart);

        public bool TryGetViewPart<T>(ViewPartKey key, out T view) where T : IView
        {
            if (TryGetMonoPartView(key, out var monoPartView) && monoPartView is T convertedView)
            {
                view = convertedView;
                return true;
            }

            view = default;
            return false;
        }
    }
}
