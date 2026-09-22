using ZE.MechBattle.Navigation;

namespace ZE.MechBattle
{
    public interface IMovementDensityMap
    {
        bool TryGetDensity(IntTriangularPos tripos, out float density);
    
    }
}
