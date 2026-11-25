using Scellecs.Morpeh;
using UnityEngine;
using VContainer;

namespace ZE.MechBattle.Views
{
    public class DelayedMonoViewContainerHandler : ISpecifiedViewContainerHandler
    {
        public ViewMode ViewMode => ViewMode.DelayedMono;
        private readonly FinalViewFunctionalApplier _finalViewFunctionalApplier;
        private readonly ViewSynchronizationApplier _viewSyncApplier;
        private readonly ColouredMaterialsDepot _colouredMaterialsDepot;


        [Inject]
        public DelayedMonoViewContainerHandler(
            FinalViewFunctionalApplier finalViewFunctionalApplier,
            ViewSynchronizationApplier viewSynchronizationApplier,
            ColouredMaterialsDepot colouredMaterialsDepot
            )
        {
            _finalViewFunctionalApplier = finalViewFunctionalApplier;
            _viewSyncApplier = viewSynchronizationApplier;
            _colouredMaterialsDepot = colouredMaterialsDepot;
        }

        public bool TryGetViewPart(IViewContainer viewContainer, ViewPartKey viewPartKey, out IView viewPart)
        {
            var monoContainer = viewContainer as MonoViewContainer;
            if (monoContainer.ContainedView is IComplexView complexView && complexView.TryGetPartView(viewPartKey, out viewPart))
            {
                return true;
            }

            viewPart = default;
            return false;
        }

        public void AddViewToContainer(ISpecifiedViewContainerHandler.ViewAdditionProtocol protocol)
        {
            var monoView = protocol.View as IMonoView;
            var monoContainer = protocol.ViewContainer as MonoViewContainer;
            monoContainer.AddContainedView(monoView);

            monoView.SetParent(monoContainer.transform);
            monoView.Transform.localPosition = protocol.LocalPos;
            monoView.Transform.localRotation = protocol.LocalRot;

            // note: there is difference with viewSyncApplier:
            // at this moment view is already applied to entity, but it is container view
            if (protocol.DoFinalCheck)
                _finalViewFunctionalApplier.CheckAndApply(protocol.ViewEntity, monoView);
        }

        public void BindEntityToViewPart(Entity entity, IViewContainer viewContainer, ViewPartKey key)
        {
            var monoContainer = viewContainer as MonoViewContainer;
            if (monoContainer.ContainedView is IComplexView complexView
                && complexView.TryGetPartView(key, out var partView)
                && partView is IMonoView monoView)
            {
                _viewSyncApplier.Apply(entity, monoView, applyViewPosition: false);
            }
            else
            {
                UnityEngine.Debug.LogError($"cannot bind entity {entity.Id} to part key {key}");
            }
        }

        public void ChangeFactionColor(Entity entity, IViewContainer viewContainer, Color color)
        {
            if ((viewContainer as MonoViewContainer).ContainedView is IFactionableView factionableView)
            {
                var material = _colouredMaterialsDepot.GetColouredMaterial(ColouredMaterialType.FactionColour, color);
                factionableView.ApplyFactionMaterial(material);
            }
        }

        public void RegisterOneShotViewContainer(Entity entity, IViewContainer viewContainer)
        {
            throw new System.NotImplementedException();
        }

        bool ISpecifiedViewContainerHandler.TryGetView<T>(Entity entity, IViewContainer viewContainer, out T view)
        {
            if (viewContainer is MonoViewContainer monoViewContainer && monoViewContainer.ContainedView is T suitableView)
            {
                view = suitableView;
                return true;
            }

            view = default;
            return false;
        }
    }
}
