using UnityEngine;
using UnityEditor;
using System.IO;

namespace ZE.MechBattle
{
    public class MechChassisDataWriter : ScriptableWizard
    {
        [SerializeField] private StepSettings _stepSettings;
        public MechView mechView;
        public MechChassisData targetData;
        private int _lastStepSettingsSource;
        private const string DATA_PATH = "Game/Resources/Scriptables/MechChassisData";

        private void OnWizardCreate()
        {
            if (mechView == null)
            {
                EditorUtility.DisplayDialog("Error", "Please select a source prefab.", "OK");
                return;
            }

            var success = true;
            targetData ??= CreateNewData();
            success = targetData.TryUpdateData(mechView, _stepSettings);

            if (success)
            {
                EditorUtility.SetDirty(targetData);
                AssetDatabase.SaveAssets();

                EditorUtility.DisplayDialog("Success",
                    $"Data successfully {(targetData == null ? "created" : "updated")}!\n" +
                    $"Asset: {AssetDatabase.GetAssetPath(targetData)}",
                    "OK");
            }
        }


        private MechChassisData CreateNewData()
        {
            var newData = CreateInstance<MechChassisData>();
            var fileName = mechView.name;

            var fullPath = Path.Combine(DATA_PATH, $"{fileName}.asset");
            if (!Directory.Exists(DATA_PATH))
                Directory.CreateDirectory(DATA_PATH);

            AssetDatabase.CreateAsset(newData, fullPath);

            return newData;
        }

        private void OnWizardUpdate()
        {
            isValid = mechView != null;

            if (!isValid)
            {
                errorString = "Please select a source prefab.";
            }
            else
            {
                errorString = "";
                if (targetData != null)
                {
                    var sourceId = targetData.GetInstanceID();
                    if (sourceId != _lastStepSettingsSource)
                    {
                        _stepSettings = targetData.StepSettings;
                        _lastStepSettingsSource = sourceId;
                    }                        
                    createButtonName = "Update Data";
                }                    
                else
                {
                    _stepSettings = default;
                    if (_lastStepSettingsSource != -1)
                    {
                        _stepSettings = default;
                        _lastStepSettingsSource = -1;
                    }
                    createButtonName = "Create Data";
                }
            }
        }

        [MenuItem("Tools/Mech chassis data writer")]
        private static void ShowWizard()
        {
            var wizard = ScriptableWizard.DisplayWizard<MechChassisDataWriter>(
                "Mech chassis data writer", 
                "Update Data"
            );
        }
    }
}
