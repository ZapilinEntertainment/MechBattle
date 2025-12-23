#if UNITY_EDITOR
using UnityEngine;
using UnityEditor;

namespace ZE.MechBattle.Navigation
{
    public class NavigationMapDrawerEditor : Editor
    {
        SerializedProperty _mapSettingsProperty;
        SerializedProperty _testPosProperty;
        SerializedProperty _testRadiusProperty;
        SerializedProperty _highlightTriangleProperty;

        void OnEnable()
        {
            // Setup the SerializedProperties.
            _mapSettingsProperty = serializedObject.FindProperty("_mapSettings");
            _testPosProperty = serializedObject.FindProperty("_testPos");
            _testRadiusProperty = serializedObject.FindProperty("_testRadius");
            _highlightTriangleProperty = serializedObject.FindProperty("_highlightTriangle");            
        }

        public override void OnInspectorGUI()
        {
            // Update the serializedProperty - always do this in the beginning of OnInspectorGUI.
            serializedObject.Update();

            EditorGUILayout.PropertyField(_mapSettingsProperty, new GUIContent("Map settings"));
            EditorGUILayout.PropertyField(_testPosProperty, new GUIContent("Test pos"));
            EditorGUILayout.PropertyField(_testRadiusProperty, new GUIContent("Test radius"));
            EditorGUILayout.PropertyField(_highlightTriangleProperty, new GUIContent("Highlight triangle"));

            var script = target as NavigationMapDrawer;

            if (GUILayout.Button("Redraw map"))
                script.RedrawMap();

            if (GUILayout.Button("Highlight triangle"))
                script.HighlightSelectedTriangle();

            // Apply changes to the serializedProperty - always do this in the end of OnInspectorGUI.
            serializedObject.ApplyModifiedProperties();
        }
    }
}
#endif
