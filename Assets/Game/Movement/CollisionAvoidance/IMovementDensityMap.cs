using ZE.MechBattle.Navigation;

namespace ZE.MechBattle
{
    public interface IMovementDensityMap 
    {
        bool TryGetDensity(IntTriangularPos tripos, out float density);
        float GetMovementDensity(IntTriangularPos tripos) => TryGetDensity(tripos, out var density) ? density : 0f;

    }
}
