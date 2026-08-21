using Scellecs.Morpeh;
using System.Collections.Generic;
using Unity.IL2CPP.CompilerServices;
using Unity.Mathematics;
using VContainer;
using ZE.MechBattle.MechMovement;
using ZE.MechBattle.Navigation;

namespace ZE.MechBattle.Ecs {
    [Il2CppSetOption(Option.NullChecks, false)]
    [Il2CppSetOption(Option.ArrayBoundsChecks, false)]
    [Il2CppSetOption(Option.DivideByZeroChecks, false)]
    public sealed class TramplingSystem : ISystem 
    {
        public World World { get; set;}

        private Stash<TriangularPosComponent> _triangularPositions;
        private Stash<StepProgressionComponent> _stepProgression;

        private readonly float _triangleHeight;
        private readonly float _hexEdgeLength;
        private readonly IMechStepsMap _stepsMap;
        private readonly IUnitsGrid _unitsGrid;
        private readonly MechMovementHandler _mechHandler;
        private readonly DamageRequestsFactory _damageRequestsFactory;

        private readonly HashSet<int2> _affectedHexes = new(4);
        private readonly Dictionary<IntTriangularPos, Entity> _affectedTripos = new(32);
        private readonly Dictionary<Entity, bool> _isStepTrampling = new(4);

        private const float TRAMPLING_LIMIT = 0.9f;
        

        [Inject]
        public TramplingSystem(
            IMechStepsMap mechStepsMap, 
            IUnitsGrid unitsGrid, 
            INavigationMap navMap,
            DamageRequestsFactory damageRequestsFactory,
            MechMovementHandler mechHandler)
        {
            _stepsMap = mechStepsMap;
            _unitsGrid = unitsGrid;
            _damageRequestsFactory = damageRequestsFactory;
            _mechHandler = mechHandler;

            _triangleHeight = navMap.TriangleHeight;
            _hexEdgeLength = navMap.HexEdgeLength;
        }

        public void OnAwake() 
        {
            _triangularPositions = World.GetStash<TriangularPosComponent>();
            _stepProgression = World.GetStash<StepProgressionComponent>();
        }

        public void OnUpdate(float deltaTime) 
        {
            if (_stepsMap.IsEmpty)
                return;

            foreach (var kvp in _stepsMap)
            {
                var footEntity = kvp.Value;

                if (!IsFootCloseToGround(footEntity))
                    continue;

                _affectedTripos.Add(kvp.Key, kvp.Value);
                var hexCoord = TriangularMath.TriangularToHex(kvp.Key, _triangleHeight, _hexEdgeLength);
                _affectedHexes.Add(hexCoord);
            }

            if (_affectedHexes.Count != 0)
            {
                foreach (var hexCoord in _affectedHexes)
                {
                    if (!_unitsGrid.TryGetUnitsInHex(hexCoord, out var unitsList))
                        continue;

                    foreach (var unit in unitsList)
                    {
                        var tripos = _triangularPositions.Get(unit).Value;
                        if (_affectedTripos.TryGetValue(tripos, out var footEntity))
                            OnUnitTrampled(footEntity, unit, tripos);
                    }
                }

                _affectedHexes.Clear();
                _affectedTripos.Clear();
            }

            _isStepTrampling.Clear();
        }

        public void Dispose() { }

        private bool IsFootCloseToGround(Entity footEntity)
        {
            if (!_isStepTrampling.TryGetValue(footEntity, out var isTrampling))
            {
                var chassisEntity = _mechHandler.GetFootChassisEntity(footEntity);
                var progressComponent = _stepProgression.Get(chassisEntity, out var progressionComponentExists);
                var progress = progressionComponentExists ? progressComponent.Progress : -1f;
                isTrampling = progress > TRAMPLING_LIMIT;
                _isStepTrampling.Add(footEntity, isTrampling);
            }
            return isTrampling;
        }

        private void OnUnitTrampled(Entity footEntity, Entity unitEntity, IntTriangularPos tripos)
        {
            _damageRequestsFactory.Build(footEntity, unitEntity, new DamageApplyParameters(DamageType.Trampling, 100f));
        }
    }
}