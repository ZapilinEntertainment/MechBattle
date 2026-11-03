using Scellecs.Morpeh;
using Unity.IL2CPP.CompilerServices;

namespace ZE.MechBattle.Ecs {
    [System.Serializable]
    [Il2CppSetOption(Option.NullChecks, false)]
    [Il2CppSetOption(Option.ArrayBoundsChecks, false)]
    [Il2CppSetOption(Option.DivideByZeroChecks, false)]
    public struct ExplosionCellAffectionComponent : IComponent 
    {
        public readonly float Damage;
        public readonly Entity CasterEntity;

        public ExplosionCellAffectionComponent(Entity caster, float damage)
        {
            CasterEntity = caster;
            Damage = damage;
        }

        public static ExplosionCellAffectionComponent operator+ (ExplosionCellAffectionComponent A, ExplosionCellAffectionComponent B) => 
            new(A.Damage > B.Damage ? A.CasterEntity : B.CasterEntity,  A.Damage + B.Damage);
    
    }
}