using Scellecs.Morpeh;
using System;
using Unity.Mathematics;

namespace ZE.MechBattle
{
    public readonly struct CellMovementData : IComparable<CellMovementData>
    {
        public readonly Entity Entity;
        public readonly float2 MoveVector; // zero if inner cell of occupation zone or entity is not moving
        public readonly int ProjectionStepIndex; // 0 is current object position, 1+ is next pos projections
        public readonly MovementCollisionAvoidancePriority Priority;

        public bool IsRealOccupationCell => ProjectionStepIndex == 0; // other can be virtual = projection of move speed

        public CellMovementData(Entity entity, MovementCollisionAvoidancePriority priority, float2 moveVector, int projectionIndex)
        {
            Entity = entity;
            Priority = priority;
            MoveVector = moveVector;
            ProjectionStepIndex = projectionIndex;
        }

        public static readonly CellMovementData Default = new(default, MovementCollisionAvoidancePriority.None, float2.zero, 0);

        public int CompareTo(CellMovementData other)
        {
            var priorityCheck = Priority.CompareTo(other.Priority);
            if (priorityCheck != 0)
                return priorityCheck;

            return other.ProjectionStepIndex.CompareTo(ProjectionStepIndex);
        }

        public static bool operator >(CellMovementData a, CellMovementData b)
    => a.CompareTo(b) > 0;

        public static bool operator <(CellMovementData a, CellMovementData b)
            => a.CompareTo(b) < 0;

        public static bool operator >=(CellMovementData a, CellMovementData b)
            => a.CompareTo(b) >= 0;

        public static bool operator <=(CellMovementData a, CellMovementData b)
            => a.CompareTo(b) <= 0;
    }
}
