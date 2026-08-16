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

        public void RegisterWindow<T>(T windowPrefab) where T : UiWindow
        {
            var instance = GameObject.Instantiate(windowPrefab, _uiRoot.DisabledWindowsContainer);
            _windows[typeof(T)] = instance;
            //UnityEngine.Debug.Log("registered " + typeof(T).ToString());
        }            

        public T ShowWindow<T>() where T : UiWindow
        {
            var window = GetWindow<T>();         
            window.transform.SetParent(_uiRoot.ActiveWindowsContainer, false);
            window.transform.SetAsLastSibling();

            return window;
        }

        public T GetWindow<T>() where T : UiWindow
        {
            var type = typeof(T);
            return (T)_windows[type];
        }

        public void HideWindow<T> (T window) where T : UiWindow
        {
            if (_uiRoot == null)
                return;
            window.transform.SetParent(_uiRoot.DisabledWindowsContainer, false);
        }
        
    }
}
