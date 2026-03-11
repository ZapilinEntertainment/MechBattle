using Unity.Mathematics;

namespace ZE.MechBattle.Navigation
{
    public struct CalculatedNavigationData
    {
        public int Cost;
        public int2 Parent;
        public int StepsCount;
    }
}
