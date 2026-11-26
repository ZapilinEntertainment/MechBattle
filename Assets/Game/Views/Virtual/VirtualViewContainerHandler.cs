using Scellecs.Morpeh;
using UnityEngine;

namespace ZE.MechBattle.Views
{
    public class VirtualViewContainerHandler : ISpecifiedViewContainerHandler
    {
        public ViewMode ViewMode => ViewMode.Virtual;

        // todo: atm single virtual views have no containers, and no regular functional
        // it should be changed if any need appears

        public void AddViewToContainer(ISpecifiedViewContainerHandler.ViewAdditionProtocol protocol)
        {
            throw new System.NotImplementedException();
        }

        public bool TryGetViewPart(IViewContainer viewContainer, ViewPartKey viewPartKey, out IView viewPart)
        {
            
            throw new System.NotImplementedException();
        }

        public void BindEntityToViewPart(Entity entity, IViewContainer viewContainer, ViewPartKey key)
        {
            throw new System.NotImplementedException();
        }

        public void ChangeFactionColor(Entity entity, IViewContainer viewContainer, Color color)
        {
            throw new System.NotImplementedException();
        }

        public void RegisterOneShotViewContainer(Entity entity, IViewContainer viewContainer)
        {
            throw new System.NotImplementedException();
        }

        bool ISpecifiedViewContainerHandler.TryGetView<T>(Entity entity, IViewContainer viewContainer, out T view)
        {
            throw new System.NotImplementedException();
        }

        public void OnFinalViewApplied(Entity entity, IViewContainer view)
        {
            throw new System.NotImplementedException();
        }
    }
}
