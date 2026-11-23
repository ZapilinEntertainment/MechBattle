using Scellecs.Morpeh;
using VContainer;
using ZE.MechBattle.Views;

namespace ZE.MechBattle
{
    public class SceneViewsContainerHandler : ISpecifiedViewContainerHandler
    {
        public ViewMode ViewMode => ViewMode.SceneMono;
        private readonly ViewSynchronizationApplier _viewSyncApplier;

        [Inject]
        public SceneViewsContainerHandler(ViewSynchronizationApplier viewSynchronizationApplier)
        {
            _viewSyncApplier = viewSynchronizationApplier;
        }

        public void AddViewToContainer(ISpecifiedViewContainerHandler.ViewAdditionProtocol protocol)
        {
            throw new System.NotImplementedException();
        }

        public void BindEntityToViewPart(Entity entity, IViewContainer viewContainer, ViewPartKey key)
        {
            if (TryGetViewPart(viewContainer, key, out var viewPart))
            {
                var monoView = viewPart as IMonoView;
                _viewSyncApplier.Apply(entity, monoView, applyViewPosition: true, doViewChecks: true);
            }                  
        }

        public bool TryGetViewPart(IViewContainer viewContainer, ViewPartKey viewPartKey, out IView viewPart)
        {
            if (viewContainer is IComplexView complexView && complexView.TryGetPartView(viewPartKey, out viewPart))
                return true;

            viewPart = default;
            return false;
        }
    }
}
