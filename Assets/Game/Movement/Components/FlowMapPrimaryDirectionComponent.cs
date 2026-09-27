using Scellecs.Morpeh;
using Unity.IL2CPP.CompilerServices;

namespace ZE.MechBattle.Ecs {
    [System.Serializable]
    [Il2CppSetOption(Option.NullChecks, false)]
    [Il2CppSetOption(Option.ArrayBoundsChecks, false)]
    [Il2CppSetOption(Option.DivideByZeroChecks, false)]
    public readonly struct FlowMapPrimaryDirectionComponent : IComponent 
    {
        public readonly byte PrimaryDirection;
        
        public FlowMapPrimaryDirectionComponent(int dir)
        {
            PrimaryDirection = (byte)dir;
        }

    }
}