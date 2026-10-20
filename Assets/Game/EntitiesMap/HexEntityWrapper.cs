using Scellecs.Morpeh;
using System.Collections.Generic;
using Unity.Mathematics;

namespace ZE.MechBattle.Navigation.Ecs
{
    public class HexEntityWrapper : IUpdatableNavigationHex
    {
        private readonly Entity _entity;
        private readonly HexEntitiesHandler _handler;
        private readonly NavigationHexPosition _pos;

        public HexEntityWrapper(int2 hexCoord, Entity entity, HexEntitiesHandler handler, INavigationMap map)
        {
            _entity = entity;
            _handler = handler;
            Pos = new(hexCoord, map);

            Exits = new();
        }

        public NavigationHexPosition Pos { get; private set; }
        public HashSet<int> Exits { get; private set; }

        public int PassabilityVersion => _handler.GetPassabilityVersion(_entity);

        public float3 CenterPos3DWorld => _pos.CenterPos3DWorld;
        public float2 CenterPosWorld => _pos.CenterPosWorld;

        public int2 HexCoordinate => _pos.HexCoordinate;


        public IReadOnlyCollection<int> PortalExitIds => Exits;

        public void UpdatePassabilityVersion() => _handler.IncreasePassabilityVersion(_entity);
    }
}
