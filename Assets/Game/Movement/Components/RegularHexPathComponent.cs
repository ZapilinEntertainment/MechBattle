using Scellecs.Morpeh;
using UnityEngine;
using Unity.IL2CPP.CompilerServices;
using ZE.MechBattle.Navigation;

namespace ZE.MechBattle.Ecs 
{

    [System.Serializable]
    [Il2CppSetOption(Option.NullChecks, false)]
    [Il2CppSetOption(Option.ArrayBoundsChecks, false)]
    [Il2CppSetOption(Option.DivideByZeroChecks, false)]
    public struct RegularHexPathComponent : IComponent 
    {
        public int PathId;
        public int StepIndex;    
        public int StepsCount;

        public RegularHexPathComponent(HexPathSearchResultData searchData)
        {
            PathId = searchData.PathId;
            StepIndex = 0;
            StepsCount = searchData.PathNodesCount;
        }
    }
}