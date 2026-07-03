using System.Collections.Generic;
using Scellecs.Morpeh;
using Unity.Mathematics;
using ZE.MechBattle.Navigation;

namespace ZE.MechBattle
{
    public readonly struct CellMovementData
    {
        public readonly Entity Entity;
        public readonly float3 MoveVector; // zero if inner cell of occupation zone or entity is not moving
        public readonly int ProjectionStepIndex; // 0 is current object position, 1+ is next pos projections
        public readonly MovementCollisionAvoidancePriority Priority;

        public CellMovementData(Entity entity, MovementCollisionAvoidancePriority priority, float3 moveVector, int projectionIndex)
        {
            Entity = entity;
            Priority = priority;
            MoveVector = moveVector;
            ProjectionStepIndex = projectionIndex;
        }
    }

    public class MovementCellsList : Dictionary<IntTriangularPos, CellMovementData>
    {
        public bool TryWriteCell(IntTriangularPos tripos, CellMovementData newData)
        {
            if (TryGetValue(tripos, out var currentData) && (currentData.ProjectionStepIndex == 0 || currentData.Priority >= newData.Priority))
            {
                return false;
            }

            Add(tripos, newData);
            return true;
        }
    }
}