#if UNITY_EDITOR
using UnityEngine;
using UnityEditor;

namespace ZE.MechBattle.Navigation
{
    [CustomEditor(typeof(NavigationMapDrawer))]
    public class NavigationMapDrawerEditor : Editor
    {
        SerializedProperty _hexEdgeSizeProperty;
        SerializedProperty _bottomLeftCornerProperty;
        SerializedProperty _topRightCornerProperty;

        void OnEnable()
        {
            // Setup the SerializedProperties.
            _hexEdgeSizeProperty = serializedObject.FindProperty("_hexEdgeSize");
            _bottomLeftCornerProperty = serializedObject.FindProperty("_bottomLeftCorner");
            _topRightCornerProperty = serializedObject.FindProperty("_topRightCorner");
        }

        public override void OnInspectorGUI()
        {
            // Update the serializedProperty - always do this in the beginning of OnInspectorGUI.
            serializedObject.Update();

            EditorGUILayout.PropertyField(_hexEdgeSizeProperty, new GUIContent("Hex edge size"));
            EditorGUILayout.PropertyField(_bottomLeftCornerProperty, new GUIContent("Bottom left corner"));
            EditorGUILayout.PropertyField(_topRightCornerProperty, new GUIContent("Top right corner"));

            if (GUILayout.Button("Redraw map"))
                (target as NavigationMapDrawer).RedrawMap();

            // Apply changes to the serializedProperty - always do this in the end of OnInspectorGUI.
            serializedObject.ApplyModifiedProperties();
        }
    }
}
#endif
