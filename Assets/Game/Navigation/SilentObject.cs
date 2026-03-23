using UnityEngine;

namespace ZE.MechBattle.Navigation
{

    [ExecuteInEditMode]
    public class SilentObject : MonoBehaviour
    {
        void OnEnable()
        {
            gameObject.hideFlags = HideFlags.DontSaveInEditor;
        }
    }
}
