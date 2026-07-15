using UnityEditor;
using UnityEngine;
using ZE.MechBattle.Vfx;

namespace ZE.MechBattle.Editor
{
    public static class ShowScriptablesMenu
    {
        private const string SCRIPTABLES_MENU = "Scriptables/";
        private const string SCRIPTABLES_FOLDER = "Scriptables/";

        [MenuItem(SCRIPTABLES_MENU + "ProjectilesData")]        
        private static void SelectProjectilesData() => SelectScriptable<ProjectilesData>();

        [MenuItem(SCRIPTABLES_MENU + "VfxData")]
        private static void SelectVfxData() => SelectScriptable<VfxData>();

        private static void SelectScriptable<T>() where T : ScriptableObject
        {
            var name = typeof(T).Name;
            T asset = UnityEditor.AssetDatabase.LoadAssetAtPath<T>($"Assets/Game/Resources/Scriptables/{name}.asset");
            if (asset == null)
                UnityEngine.Debug.LogError(name + " scriptable not exist");
            else
                UnityEditor.Selection.activeObject = asset;
        }
    }

}
