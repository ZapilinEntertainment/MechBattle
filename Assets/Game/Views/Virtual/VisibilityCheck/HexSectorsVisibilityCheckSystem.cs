using Scellecs.Morpeh;
using System.Collections.Generic;
using Unity.IL2CPP.CompilerServices;
using Unity.Mathematics;

namespace ZE.MechBattle.Ecs {

    public interface IVisibleHexList
    {
        bool IsHexVisible(int2 hexCoord, out int sectorsVisibilityMask);
        IReadOnlyDictionary<int2, int> VisibleSectors { get; }
    }

    [Il2CppSetOption(Option.NullChecks, false)]
    [Il2CppSetOption(Option.ArrayBoundsChecks, false)]
    [Il2CppSetOption(Option.DivideByZeroChecks, false)]
    public sealed class HexSectorsVisibilityCheckSystem : ILateSystem,IVisibleHexList
    {
        public World World { get; set;}
        public IReadOnlyDictionary<int2, int> VisibleSectors => _visibleSectors;

        private readonly Dictionary<int2, int> _visibleSectors = new(64);
        private Filter _hexesFilter;
        private Stash<HexComponent> _hexComponents;
        private Stash<HexSectorsVisibilityMaskComponent> _hexSectorsVisibilities;

        public void OnAwake() 
        {
            _hexesFilter = World.Filter
                .With<HexComponent>()
                .With<HexSectorsVisibilityMaskComponent>()
                .Build();
            _hexComponents = World.GetStash<HexComponent>();
            _hexSectorsVisibilities = World.GetStash<HexSectorsVisibilityMaskComponent>();
        }

        public void OnUpdate(float deltaTime) 
        {
            if (_hexesFilter.IsEmpty())
            {
                if (_visibleSectors.Count != 0)
                    _visibleSectors.Clear();
                return;
            }

            _visibleSectors.Clear();
            foreach (var entity in _hexesFilter)
            {
                var mask = _hexSectorsVisibilities.Get(entity).Value;
                if (mask == 0)
                    continue;

                var hexCoord = _hexComponents.Get(entity).Coord;
                _visibleSectors.Add(hexCoord, mask);
            }
        }

        public void Dispose() { }

        public bool IsHexVisible(int2 hexCoord, out int sectorsVisibilityMask) => _visibleSectors.TryGetValue(hexCoord, out sectorsVisibilityMask);
    }
}