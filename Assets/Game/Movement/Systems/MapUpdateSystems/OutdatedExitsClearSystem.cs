using Scellecs.Morpeh;
using VContainer;
using Unity.IL2CPP.CompilerServices;
using ZE.MechBattle.Navigation;

namespace ZE.MechBattle.Ecs {
    [Il2CppSetOption(Option.NullChecks, false)]
    [Il2CppSetOption(Option.ArrayBoundsChecks, false)]
    [Il2CppSetOption(Option.DivideByZeroChecks, false)]
    public sealed class OutdatedExitsClearSystem : ISystem 
    {
        public World World { get; set;}
        private readonly OutdatedExitsList _outdatedExitsList;
        private readonly OutdatedPortalsList _outdatedPortalsList;
        private readonly IExitsLogic _exitLogic;
        private readonly IPortalExitsList _exitsList;
        private readonly IHexPortalsList _portalsList;

        [Inject]
        public OutdatedExitsClearSystem(
            OutdatedExitsList outdatedExitsList, 
            OutdatedPortalsList outdatedPortalsList,
            IExitsLogic exitsLogic, 
            IPortalExitsList exitsList, 
            IHexPortalsList portalsList)
        {
            _outdatedExitsList = outdatedExitsList;
            _outdatedPortalsList = outdatedPortalsList;
            _exitLogic = exitsLogic;
            _exitsList = exitsList;
            _portalsList = portalsList;
        }

        public void OnAwake() { }

        public void Dispose() { }

        public void OnUpdate(float deltaTime) 
        {
            if (_outdatedExitsList.Count == 0)
                return;

            foreach (var exitId in _outdatedExitsList)
            {
                _exitLogic.RemoveExit(exitId);
            }
            _outdatedExitsList.Clear();


            foreach (var portalKvp in _portalsList)
            {
                var portalData = portalKvp.Value;
                if (!_exitsList.ContainsKey(portalData.ExitIdA) || !_exitsList.ContainsKey(portalData.ExitIdB))
                {
                    _outdatedPortalsList.Add(portalKvp.Key);
                    continue;
                }                    
            }
        }        
    }
}