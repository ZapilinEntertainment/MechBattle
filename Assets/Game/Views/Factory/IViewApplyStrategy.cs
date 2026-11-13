using Scellecs.Morpeh;

namespace ZE.MechBattle.Views
{
    public interface IViewApplyStrategy
    {
        ViewMode RealizingViewType { get; }
        void ApplyView(Entity entity, string viewId);
        IViewContainer AddEmptyViewContainer(Entity entity);
    
    }
}
