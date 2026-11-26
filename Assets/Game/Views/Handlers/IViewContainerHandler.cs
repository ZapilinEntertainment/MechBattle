using Scellecs.Morpeh;
using Unity.Mathematics;
using UnityEngine;
using ZE.MechBattle.Views;

namespace ZE.MechBattle
{
    public interface IViewContainerHandler
    {
        void AddViewToContainer(Entity viewEntity, IView view, int containerId, bool doFinalCheck);
        void BindEntityToViewPart(Entity entity, int containerId, ViewPartKey key);
        void ChangeFactionColor(Entity entity, int containerId, Color color);
        bool TryGetViewPart(int containerId, ViewPartKey viewPartKey, out IView viewPart);
        void RegisterOneShotViewContainer(Entity entity, IView view);
        bool TryGetView<T>(Entity entity, out T view) where T : class, IView;
        void OnFinalViewApplied(Entity entity, int containerId);
    }
}

namespace ZE.MechBattle.Views
{
    public interface ISpecifiedViewContainerHandler
    { 
        ViewMode ViewMode { get; }


        public struct ViewAdditionProtocol
        {
            public Entity ViewEntity;
            public IView View;
            public IViewContainer ViewContainer;
            public float3 LocalPos;
            public quaternion LocalRot;
        }
        void AddViewToContainer(ViewAdditionProtocol protocol);
        void BindEntityToViewPart(Entity entity, IViewContainer viewContainer, ViewPartKey key);
        void ChangeFactionColor(Entity entity, IViewContainer viewContainer, Color color);
        bool TryGetViewPart(IViewContainer viewContainer, ViewPartKey viewPartKey, out IView viewPart);
        void RegisterOneShotViewContainer(Entity entity, IViewContainer viewContainer);
        bool TryGetView<T>(Entity entity, IViewContainer viewContainer, out T view) where T : class,IView;
        void OnFinalViewApplied(Entity entity, IViewContainer view);
    }
}
