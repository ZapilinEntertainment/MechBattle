using Unity.IL2CPP.CompilerServices;
using TriInspector;

namespace ZE.MechBattle.Ecs 
{

    [System.Serializable]
    [Il2CppSetOption(Option.NullChecks, false)]
    [Il2CppSetOption(Option.ArrayBoundsChecks, false)]
    [Il2CppSetOption(Option.DivideByZeroChecks, false)]
    public struct HexPathIdComponent : IPathUserComponent<int>, IHexPathComponent
    {
        [ReadOnly, ShowInInspector]public readonly int PathId;
        public int PathKey => PathId;


        public HexPathIdComponent(int pathId)
        {
            PathId = pathId;
        }
    }
}