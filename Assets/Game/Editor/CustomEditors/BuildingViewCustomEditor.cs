using UnityEditor;
using ZE.MechBattle;

[CustomEditor(typeof(BuildingView), true)]
public class BuildingViewEditor : Editor
{
    void OnSceneGUI()
    {
        var buildingView = (BuildingView)target;
        if (buildingView.TryGetBlockZoneConfig(out var blockConfig))
        {
            var size = blockConfig.DecodeSize2D();
            ZoneDrawer.Draw(new(buildingView.transform.rotation, buildingView.transform.position),size , blockConfig.Mode);
        }
        
    }
}
