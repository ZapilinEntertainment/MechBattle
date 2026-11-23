using Scellecs.Morpeh;
using ZE.MechBattle.Ecs;

namespace ZE.MechBattle
{
    // class, because will be transferred as interface (IView/ IVirtualView)
    public class VirtualView : IVirtualView
    {
        public Entity Entity => _entity;
        public VirtualViewComponent ViewData => _viewComponent;


        private readonly Entity _entity;
        private readonly VirtualViewComponent _viewComponent;

        public VirtualView(Entity entity, VirtualViewComponent viewComponent)
        {
            _entity = entity;
            _viewComponent = viewComponent;
        }

        public void Dispose() { }
    }
}
