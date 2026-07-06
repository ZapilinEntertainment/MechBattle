using Scellecs.Morpeh;
using UnityEngine;
using Unity.IL2CPP.CompilerServices;
using ZE.MechBattle.Navigation;

namespace ZE.MechBattle.Ecs {
    [System.Serializable]
    [Il2CppSetOption(Option.NullChecks, false)]
    [Il2CppSetOption(Option.ArrayBoundsChecks, false)]
    [Il2CppSetOption(Option.DivideByZeroChecks, false)]
    public readonly struct TrianglePathSearchRequestComponent : ITrianglePathComponent
    {
        public readonly IntTriangularPos Start;
        public readonly IntTriangularPos End;

        public TrianglePathSearchRequestComponent (IntTriangularPos start, IntTriangularPos end)
        {
            Start = start;
            End = end;
        }

    }
}