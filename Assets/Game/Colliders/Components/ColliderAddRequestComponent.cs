using Scellecs.Morpeh;
using UnityEngine;
using Unity.IL2CPP.CompilerServices;

namespace ZE.MechBattle.Ecs {
    [System.Serializable]
    [Il2CppSetOption(Option.NullChecks, false)]
    [Il2CppSetOption(Option.ArrayBoundsChecks, false)]
    [Il2CppSetOption(Option.DivideByZeroChecks, false)]
    public readonly struct ColliderAddRequestComponent : IComponent 
    {
        public readonly Entity TargetHostEntity;
        public readonly Entity ColliderOwnerEntity;
        public readonly ColliderSetupInfo ColliderSetupInfo;

        public ColliderAddRequestComponent(Entity hostEntity, Entity ownerEntity, ColliderSetupInfo setupInfo)
        {
            TargetHostEntity = hostEntity;
            ColliderOwnerEntity = ownerEntity;
            ColliderSetupInfo = setupInfo;
        }
    }
}