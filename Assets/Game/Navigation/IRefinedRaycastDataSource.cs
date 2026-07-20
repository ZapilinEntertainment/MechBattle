using Unity.Collections;

namespace ZE.MechBattle.Navigation
{
    public interface IRefinedRaycastDataSource
    {
        static int GetArrayLength(MapSettings settings) => settings.TrianglesCountInHex;
        void CopyRefinedRaycastDataInto(NativeArray<RefinedTriangleRaycastData> data);
    }
}
