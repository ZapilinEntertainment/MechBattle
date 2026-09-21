using System.Collections.Generic;
using Unity.Mathematics;
using VContainer;

namespace ZE.MechBattle
{
    public class MovementDensityMapsManager 
    {
        private readonly Dictionary<int2, MovementDensityMap> _maps = new();
        private readonly FlowMapsFactory _factory;

        [Inject]
        public MovementDensityMapsManager(FlowMapsFactory flowMapsFactory)
        {
            _factory = flowMapsFactory;
        }

        public MovementDensityMap GetDensityMap(int2 hexCoord)
        {
            if (!_maps.TryGetValue(hexCoord, out var densityMap))
            {
                densityMap = _factory.CreateDensityMap(hexCoord);
                _maps.Add(hexCoord, densityMap);
            }

            return densityMap;
        }
    
    }
}
