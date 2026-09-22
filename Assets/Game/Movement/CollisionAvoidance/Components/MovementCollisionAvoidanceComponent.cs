using Scellecs.Morpeh;
using UnityEngine;
using Unity.IL2CPP.CompilerServices;

namespace ZE.MechBattle.Ecs {
    [System.Serializable]
    [Il2CppSetOption(Option.NullChecks, false)]
    [Il2CppSetOption(Option.ArrayBoundsChecks, false)]
    [Il2CppSetOption(Option.DivideByZeroChecks, false)]
    public struct MovementCollisionAvoidanceComponent : IComponent 
    {
        public readonly float RadiusInUnits;
        public readonly MovementCollisionAvoidancePriority Priority;

        public MovementCollisionAvoidanceComponent(MovementCollisionAvoidancePriority priority, float radiusInUnits = 0f)
        {
            Priority = priority;
            RadiusInUnits = radiusInUnits;
        }
    
    }
}