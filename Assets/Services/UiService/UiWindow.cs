using System;
using UnityEngine;
using ZE.Utils;

namespace ZE.UiService
{
    public abstract class UiWindow : MonoBehaviour, IDisposable
    {
        private WindowsManager _windowsManager;
        public static string GetWindowDefaultAssetName<T>() where T : UiWindow => StringExtension.ToSnakeCaseWithAcronyms(typeof(T).Name);
        public string GetWindowDefaultAssetName() => StringExtension.ToSnakeCaseWithAcronyms( GetType().Name);


        public void AssignWindowsManager(WindowsManager windowsManager) => _windowsManager = windowsManager;

        public void Dispose() => _windowsManager.HideWindow(this);
    }
}
