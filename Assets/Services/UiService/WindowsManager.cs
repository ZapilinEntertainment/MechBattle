using System;
using System.Collections.Generic;
using UnityEngine;
using VContainer;

namespace ZE.UiService
{
    public class WindowsManager
    {
        private readonly UiRoot _uiRoot;
        private readonly Dictionary<Type, UiWindow> _windows = new();

        [Inject]
        public WindowsManager(UiRoot uiRoot)
        {
            _uiRoot = uiRoot;
        }

        public void RegisterWindow(UiWindow windowInstance) => _windows[windowInstance.GetType()] = windowInstance;

        public T ShowWindow<T>() where T : UiWindow
        {
            var type = typeof(T);
            var window = _windows[type];           
            window.transform.SetParent(_uiRoot.ActiveWindowsContainer, false);
            window.transform.SetAsLastSibling();

            return (T)window;
        }

        public void HideWindow<T> (T window) where T : UiWindow
        {
            if (_uiRoot == null)
                return;
            window.transform.SetParent(_uiRoot.DisabledWindowsContainer, false);
        }
        
    }
}
