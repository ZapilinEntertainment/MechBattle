using UnityEngine;
using ZE.UiService;

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
