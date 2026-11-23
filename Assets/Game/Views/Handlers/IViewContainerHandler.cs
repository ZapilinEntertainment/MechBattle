using System;
using Scellecs.Morpeh;
using Unity.Mathematics;

namespace ZE.MechBattle.Views
{
    public interface IViewContainerHandler
    {
        void AddViewToContainer(Entity viewEntity, IView view, int containerId, bool doFinalCheck);
        void BindEntityToViewPart(Entity entity, int containerId, ViewPartKey key);
        bool TryGetViewPart(int containerId, ViewPartKey viewPartKey, out IView viewPart);
    }

    public interface ISpecifiedViewContainerHandler
    { 
        ViewMode ViewMode { get; }


        public struct ViewAdditionProtocol
        {
            public Entity ViewEntity;
            public IView View;
            public IViewContainer ViewContainer;
            public bool DoFinalCheck;
            public float3 LocalPos;
            public quaternion LocalRot;
        }
        void AddViewToContainer(ViewAdditionProtocol protocol);
        void BindEntityToViewPart(Entity entity, IViewContainer viewContainer, ViewPartKey key);
        bool TryGetViewPart(IViewContainer viewContainer, ViewPartKey viewPartKey, out IView viewPart);
    }
}
