using Scellecs.Morpeh;
using UnityEngine;
using VContainer;
using ZE.MechBattle.Ecs;
using ZE.MechBattle.Views;

namespace ZE.MechBattle
{
    public class SceneViewsContainerHandler : ISpecifiedViewContainerHandler
    {
        public ViewMode ViewMode => ViewMode.SceneMono;
        private readonly ViewSynchronizationApplier _viewSyncApplier;
        private readonly ColouredMaterialsDepot _colouredMaterialsDepot;
        private readonly IViewContainersManager _viewContainersManager;
        private readonly FinalViewFunctionalApplier _finalViewFunctionalApplier;
        private readonly Stash<ViewContainerComponent> _viewContainers;

        [Inject]
        public SceneViewsContainerHandler(
            ViewSynchronizationApplier viewSynchronizationApplier,
            ColouredMaterialsDepot colouredMaterialsDepot,
            World world,
            IViewContainersManager viewContainersManager,
            FinalViewFunctionalApplier finalViewFunctionalApplier)
        {
            _viewSyncApplier = viewSynchronizationApplier;
            _colouredMaterialsDepot = colouredMaterialsDepot;
            _viewContainersManager = viewContainersManager;
            _finalViewFunctionalApplier = finalViewFunctionalApplier;
            _viewContainers = world.GetStash<ViewContainerComponent>();
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
                _viewSyncApplier.Apply(entity, monoView, applyViewPosition: true, finalViewApplicationRequired: false);
            }                  
        }

        public bool TryGetViewPart(IViewContainer viewContainer, ViewPartKey viewPartKey, out IView viewPart)
        {
            if (viewContainer is IComplexView complexView && complexView.TryGetPartView(viewPartKey, out viewPart))
                return true;

            viewPart = default;
            return false;
        }

        public void ChangeFactionColor(Entity entity, IViewContainer viewContainer, Color color)
        {
            if ( viewContainer is IFactionableView factionableView)
            {
                var material = _colouredMaterialsDepot.GetColouredMaterial(ColouredMaterialType.FactionColour, color);
                factionableView.ApplyFactionMaterial(material);
            }                
        }

        public void RegisterOneShotViewContainer(Entity entity, IViewContainer viewContainer)
        {
            var containerId = _viewContainersManager.RegisterOneShotContainer(viewContainer);
            _viewContainers.Add(entity, new(containerId));
        }

        bool ISpecifiedViewContainerHandler.TryGetView<T>(Entity entity, IViewContainer viewContainer, out T view)
        {
            if (viewContainer is T suitableView)
            {
                view = suitableView;
                return true;
            }

            view = default;
            return false;
        }

        public void OnFinalViewApplied(Entity entity, IViewContainer view)
        {
            _finalViewFunctionalApplier.CheckMonoViewAndApply(entity, view as IMonoView);
        }
    }
}
