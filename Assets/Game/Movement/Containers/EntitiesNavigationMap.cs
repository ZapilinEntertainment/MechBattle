using Scellecs.Morpeh;
using System.Collections.Generic;
using Unity.Collections;
using Unity.Mathematics;
using VContainer;
using ZE.MechBattle.Navigation;
using ZE.MechBattle.Navigation.Ecs;

namespace ZE.MechBattle
{
    public interface IEntitiesNavigationMap : IEntitiesMap { }

    public class EntitiesNavigationMap : NavigationMapBase, IUpdatableMap, IEntitiesNavigationMap
    {
        public IEnumerable<IUpdatableNavigationHex> Hexes => _hexes.Values;
        public IEnumerable<int2> HexCoords => _hexes.Keys;

        IEnumerable<INavigationHex> INavigationMap.Hexes => Hexes;

        private Dictionary<int2, HexEntityWrapper> _hexes = new();
        private Dictionary<IntTriangularPos, Entity> _cells = new();

        private readonly CellEntitiesHandler _cellsHandler;
        private readonly HexEntitiesHandler _hexHandler;

        [Inject]
        public EntitiesNavigationMap(
            MapSettings settings, 
            CellEntitiesHandler cellEntitiesHandler,
            HexEntitiesHandler hexEntitiesHandler) 
            : base(settings, Allocator.Persistent)
        {
            _cellsHandler = cellEntitiesHandler;
            _hexHandler = hexEntitiesHandler;
        }

        #region HEXES
        public bool ContainsHex(int2 hexCoord) => _hexes.ContainsKey(hexCoord);

        public INavigationHex GetOrCreateHex(int2 hexCoord) => GetOrCreateUpdatableHex(hexCoord);

        public IUpdatableNavigationHex GetOrCreateUpdatableHex(int2 hexCoord)
        {
            if (!_hexes.TryGetValue(hexCoord, out var hexWrapper))
            {
                var entity = _hexHandler.CreateHexEntity(hexCoord);
                hexWrapper = new HexEntityWrapper(hexCoord, entity, _hexHandler, this);
                _hexes.Add(hexCoord, hexWrapper);
            }

            return hexWrapper;
        }

        public bool TryGetHex(int2 hexCoord, out INavigationHex protectedHex)
        {
            if (_hexes.TryGetValue(hexCoord, out var hexWrapper))
            {
                protectedHex = hexWrapper;
                return true;
            }
            else
            {
                protectedHex = default;
                return false;
            }
        }
        #endregion

        #region CELLS

        public override CellPassabilityData GetPassabilityData(IntTriangularPos pos)
        {
            if (_cells.TryGetValue(pos, out var cellEntity))
                return _cellsHandler.GetPassability(cellEntity, DefaultPassability);

            return NavigationLogic.GetDefaultPassability(DefaultPassability);
        }


        public void UpdateCellPassability(IntTriangularPos pos, CellPassabilityData passability) =>
            _cellsHandler.SetEntityPassability(GetOrCreateCellEntity(pos), passability);

        public override CellHeightData GetHeightData(IntTriangularPos pos)
        {
            if (_cells.TryGetValue(pos, out var cellEntity))
                return _cellsHandler.GetHeightData(cellEntity);

            return NavigationLogic.GetDefaultHeightData();
        }

        public void UpdateHeightData(IntTriangularPos pos, CellHeightData heightData) =>
            _cellsHandler.SetEntityHeight(GetOrCreateCellEntity(pos), heightData);

        private Entity GetOrCreateCellEntity(IntTriangularPos tripos)
        {
            if (!_cells.TryGetValue(tripos, out var cellEntity))
            {
                cellEntity = _cellsHandler.CreateCellEntity(tripos);
                _cells.Add(tripos, cellEntity);
            }

            return cellEntity;
        }

        public bool TryGetEntity(IntTriangularPos tripos, out Entity entity) => 
            _cells.TryGetValue(tripos, out entity);
        #endregion
    }
}
