using UnityEngine;
using ZE.UiService;
using ZE.Utils;

namespace ZE.MechBattle
{
    public interface IWindowBinder
    {
        Awaitable LoadWindow();
        void RegisterWindow(WindowsManager windowsManager);
    
    }

    public class WindowBinder<WindowType> : IWindowBinder where WindowType : UiWindow
    {
        private readonly string _assetPath;
        private WindowType _windowAsset;

        public WindowBinder() => _assetPath = UiWindow.GetWindowDefaultAssetName<WindowType>();
        public WindowBinder(string assetPath) => _assetPath = assetPath;

        public async Awaitable LoadWindow()
        {
            _windowAsset = await AssetsManager.LoadComponentAssetDirectly<WindowType>(_assetPath);
        }

        public void RegisterWindow(WindowsManager windowsManager)
        {
            windowsManager.RegisterWindow(_windowAsset);
        }
    }
}
