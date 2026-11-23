namespace ZE.MechBattle
{
    public abstract class ComplexMonoView : SimpleView, IComplexView
    {
        public abstract bool TryGetPartView(ViewPartKey key, out IMonoView viewPart);

        bool IComplexView.TryGetPartView(ViewPartKey key, out IView partView)
        {
            if (TryGetPartView(key, out var view))
            {
                partView = view;
                return true;
            }

            partView = default;
            return false;
        }
    }
}
