using System;
using System.Collections.Generic;
using Scellecs.Morpeh;
using Unity.Mathematics;
using Unity.Collections;
using Unity.Burst;
using ZE.MechBattle.Navigation;

namespace ZE.MechBattle
{
    [BurstCompile]
    public readonly struct CellMovementData
    {
        public readonly Entity Entity;
        public readonly float3 MoveVector; // zero if inner cell of occupation zone or entity is not moving
        public readonly int ProjectionStepIndex; // 0 is current object position, 1+ is next pos projections
        public readonly MovementCollisionAvoidancePriority Priority;

        public bool IsRealOccupationCell => ProjectionStepIndex == 0; // other can be virtual = projection of move speed

        public CellMovementData(Entity entity, MovementCollisionAvoidancePriority priority, float3 moveVector, int projectionIndex)
        {
            Entity = entity;
            Priority = priority;
            MoveVector = moveVector;
            ProjectionStepIndex = projectionIndex;
        }
    }

    public interface IMovementCellsMap
    {
        bool TryGetValue(IntTriangularPos tripos, out CellMovementData cellValue);
        NativeParallelHashMap<IntTriangularPos, CellMovementData>.ReadOnly AsReadonlyMap();
    }

    public class MovementCellsMap : IMovementCellsMap, IDisposable
    {
        public NativeParallelHashMap<IntTriangularPos, CellMovementData> AsNative() => _map;
        private NativeParallelHashMap<IntTriangularPos, CellMovementData> _map;        

        public bool TryGetValue(IntTriangularPos tripos, out CellMovementData cellValue) => _map.TryGetValue(tripos, out cellValue);

        public bool TryWriteCell(IntTriangularPos tripos, CellMovementData newData)
        {
            if (_map.TryGetValue(tripos, out var currentData) && (currentData.IsRealOccupationCell || currentData.Priority >= newData.Priority))
            {
                return false;
            }

            _map.Add(tripos, newData);
            return true;
        }

        public void Add(IntTriangularPos tripos, CellMovementData movementData) => _map.Add(tripos, movementData);
        public void Clear() => _map.Clear();

        public void Dispose() => _map.Dispose();

        public NativeParallelHashMap<IntTriangularPos, CellMovementData>.ReadOnly AsReadonlyMap() => _map.AsReadOnly();
    }
}