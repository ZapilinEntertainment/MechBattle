using Scellecs.Morpeh;
using UnityEngine;
using Unity.IL2CPP.CompilerServices;

namespace ZE.MechBattle.Ecs {
    [System.Serializable]
    [Il2CppSetOption(Option.NullChecks, false)]
    [Il2CppSetOption(Option.ArrayBoundsChecks, false)]
    [Il2CppSetOption(Option.DivideByZeroChecks, false)]
    public struct RepairTeamsComponent : IComponent 
    {
        public int FreeTeamsCount;
        public readonly int TotalTeamsCount;

        public RepairTeamsComponent(int teamsCount)
        {
            TotalTeamsCount = teamsCount;
            FreeTeamsCount = TotalTeamsCount;
        }
    
    }
}