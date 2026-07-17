using UnityEngine;
using Scellecs.Morpeh;
using ZE.MechBattle.Ecs;
using VContainer;

namespace ZE.MechBattle
{
    public class FactionVisibleMarksApplier
    {
        private readonly IPlayersList _playersList;
        private readonly ColouredMaterialsDepot _colouredMaterialsDepot;
        private readonly Stash<PlayerAffiliationComponent> _playerAffiliationComponents;

        [Inject]
        public FactionVisibleMarksApplier(World world, IPlayersList playersList, ColouredMaterialsDepot colouredMaterialsDepot)
        {
            _playersList = playersList;
            _colouredMaterialsDepot = colouredMaterialsDepot;
            _playerAffiliationComponents = world.GetStash<PlayerAffiliationComponent>();
        }

        public void CheckView(Entity entity, IMonoView view)
        {
            var playerAffiliationComponent = _playerAffiliationComponents.Get(entity, out var havePlayerAffiliation);
            if (havePlayerAffiliation && view is IFactionableView factionableView)
            {
                var color = _playersList.GetPlayerColor(playerAffiliationComponent.PlayerKey);
                var material = _colouredMaterialsDepot.GetColouredMaterial(ColouredMaterialType.FactionColour, color);
                factionableView.ApplyFactionMaterial(material);
            }
        }
    
    }
}
