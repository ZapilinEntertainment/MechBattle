using Scellecs.Morpeh;
using ZE.MechBattle.Ecs;
using VContainer;

namespace ZE.MechBattle
{
    public class FactionVisibleMarksApplier
    {
        private readonly FactionHandler _factionHandler;
        private readonly IViewContainerHandler _viewContainerHandler;
        private readonly Stash<PlayerAffiliationComponent> _playerAffiliationComponents;
        private readonly Stash<ViewContainerComponent> _viewContainers;

        [Inject]
        public FactionVisibleMarksApplier(World world, FactionHandler factionHandler, IViewContainerHandler viewContainerHandler)
        {
            _factionHandler = factionHandler;
            _viewContainerHandler = viewContainerHandler;

            _playerAffiliationComponents = world.GetStash<PlayerAffiliationComponent>();
            _viewContainers = world.GetStash<ViewContainerComponent>();
        }

        public void CheckView(Entity entity)
        {
            var playerAffiliationComponent = _playerAffiliationComponents.Get(entity, out var havePlayerAffiliation);
            var viewContainerComponent = _viewContainers.Get(entity, out var haveViewContainer);
            if (!havePlayerAffiliation | !haveViewContainer)
                return;

            _viewContainerHandler.ChangeFactionColor(entity, viewContainerComponent.Id, _factionHandler.GetFactionColor(playerAffiliationComponent.PlayerKey));
        }
    
    }
}
