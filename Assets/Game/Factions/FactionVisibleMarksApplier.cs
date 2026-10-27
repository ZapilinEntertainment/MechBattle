using UnityEngine;
using Scellecs.Morpeh;
using ZE.MechBattle.Ecs;
using VContainer;

namespace ZE.MechBattle
{
    public class FactionVisibleMarksApplier
    {
        private readonly FactionHandler _factionHandler;
        private readonly ColouredMaterialsDepot _colouredMaterialsDepot;
        private readonly Stash<PlayerAffiliationComponent> _playerAffiliationComponents;

        [Inject]
        public FactionVisibleMarksApplier(World world, FactionHandler factionHandler, ColouredMaterialsDepot colouredMaterialsDepot)
        {
            _factionHandler = factionHandler;
            _colouredMaterialsDepot = colouredMaterialsDepot;
            _playerAffiliationComponents = world.GetStash<PlayerAffiliationComponent>();
        }

        public void CheckView(Entity entity, IView view)
        {
            var playerAffiliationComponent = _playerAffiliationComponents.Get(entity, out var havePlayerAffiliation);
            if (havePlayerAffiliation && view is IFactionableView factionableView)
            {
                var color = _factionHandler.GetFactionColor(playerAffiliationComponent.PlayerKey);
                var material = _colouredMaterialsDepot.GetColouredMaterial(ColouredMaterialType.FactionColour, color);
                factionableView.ApplyFactionMaterial(material);
            }

            if (view is IBuildingView)
                UnityEngine.Debug.Log($"{entity.Id} {havePlayerAffiliation} : {playerAffiliationComponent.PlayerKey.Id}");
        }
    
    }
}
