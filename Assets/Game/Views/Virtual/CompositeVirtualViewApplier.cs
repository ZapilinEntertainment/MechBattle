using Scellecs.Morpeh;
using UnityEngine;
using ZE.MechBattle.Views;

namespace ZE.MechBattle
{
    public class CompositeVirtualViewApplier : IViewApplyStrategy
    {
        public ViewMode RealizingViewType => ViewMode.CompositeVirtual;

        public IViewContainer AddEmptyViewContainer(Entity entity)
        {
            throw new System.NotImplementedException();
        }

        public void ApplyView(Entity entity, string viewId)
        {
            throw new System.NotImplementedException();
        }
    }
}
