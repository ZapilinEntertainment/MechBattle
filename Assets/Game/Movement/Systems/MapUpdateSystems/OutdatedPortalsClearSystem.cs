using Scellecs.Morpeh;
using VContainer;
using Unity.IL2CPP.CompilerServices;
using ZE.MechBattle.Navigation;

namespace ZE.MechBattle.Ecs {
    [Il2CppSetOption(Option.NullChecks, false)]
    [Il2CppSetOption(Option.ArrayBoundsChecks, false)]
    [Il2CppSetOption(Option.DivideByZeroChecks, false)]
    public sealed class OutdatedPortalsClearSystem : ISystem 
    {
        public World World { get; set;}
        private readonly OutdatedPortalsList _outdatedPortalsList;
        private readonly IPortalsLogic _portalsLogic;

        [Inject]
        public OutdatedPortalsClearSystem(OutdatedPortalsList outdatedPortalsList, IPortalsLogic portalsLogic)
        {
            _outdatedPortalsList = outdatedPortalsList;
            _portalsLogic = portalsLogic;
        }

        public void OnAwake() { }
        public void Dispose() { }

        public void OnUpdate(float deltaTime) 
        {
            if (_outdatedPortalsList.Count == 0)
                return;

            foreach (var outdatedPortalId in _outdatedPortalsList)
            {
                _portalsLogic.RemovePortal(outdatedPortalId);
            }
            _outdatedPortalsList.Clear();
        }       
    }
}