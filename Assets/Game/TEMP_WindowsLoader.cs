using UnityEngine;
using ZE.UiService;
using VContainer;

namespace ZE.MechBattle
{
    public class TEMP_WindowsLoader : MonoBehaviour
    {
        [SerializeField] private UiWindow[] _windowPrefabs;

        [Inject]
        public void Inject(WindowsManager manager, UiRoot root)
        {
            foreach (var windowPrefab in _windowPrefabs)
            {
                var window = GameObject.Instantiate(windowPrefab, root.DisabledWindowsContainer);
                manager.RegisterWindow(window);
            }
        }
    }
}
