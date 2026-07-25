using Scellecs.Morpeh;
using ZE.MechBattle.Navigation;

namespace ZE.MechBattle
{
    public readonly struct MechStepOccupationData
    {
        public readonly IntTriangularPos Tripos;
        public readonly Entity Entity;    
        public MechStepOccupationData(IntTriangularPos tripos, Entity entity)
        {
            Tripos = tripos;
            Entity = entity;
        }
    }
}
