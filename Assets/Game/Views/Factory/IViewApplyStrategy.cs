using Scellecs.Morpeh;

namespace ZE.MechBattle.Views
{
    public interface IViewApplyStrategy
    {
        ViewMode RealizingViewType { get; }
        void ApplyView(Entity entity, string viewId);
        IViewContainer AddEmptyViewContainer(Entity entity);

        public static ViewMode ViewIdToStrategyType(string viewId)
        {
            if (viewId.StartsWith(ViewConstants.VIRTUAL_VIEW_PREFIX, System.StringComparison.Ordinal))
                return ViewMode.Virtual;
            else
                return ViewMode.DelayedMono;
        }
    
    }
}
