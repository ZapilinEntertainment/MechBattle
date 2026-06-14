using System;
using UnityEngine;

namespace ZE.Utils
{
    [DefaultExecutionOrder(-1000)]
    [DisallowMultipleComponent]
    public class EditorPlaymodeLifetimeObject : MonoBehaviour 
    {
        public static bool IsQuitting { get; private set; } = false;
        private static EditorPlaymodeLifetimeObject s_instance;
       
        public void Awake()
        {
            if (s_instance != null)
                return;

            s_instance = this;

#if UNITY_EDITOR
            UnityEditor.EditorApplication.playModeStateChanged += 
                (mode) => { if (mode == UnityEditor.PlayModeStateChange.ExitingPlayMode) IsQuitting = true;};
#endif
        }
    }
}
