using Scellecs.Morpeh;
using UnityEngine;
using Unity.IL2CPP.CompilerServices;

namespace ZE.MechBattle.Ecs {
    [System.Serializable]
    [Il2CppSetOption(Option.NullChecks, false)]
    [Il2CppSetOption(Option.ArrayBoundsChecks, false)]
    [Il2CppSetOption(Option.DivideByZeroChecks, false)]
    public readonly struct AttackDistanceComponent : IComponent 
    {
        public readonly float Recommended;
        public readonly float Maximum;
        //todo: add minimum

        public float RecommendedSq => Recommended * Recommended;
        public float MaximumSq => Maximum * Maximum;
        
        public AttackDistanceComponent(float recommended, float max) 
        {
            Recommended = recommended;
            Maximum = max;
        }
    
    }
}