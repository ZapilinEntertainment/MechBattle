using UnityEngine;
using VContainer;
using ZE.MechBattle.UI;
using ZE.UiService;

namespace ZE.MechBattle
{
    public class WeaponTargetMarkerFactory
    {
        // todo: add inner pool

        private UIWeaponAimMarker _prefab;
        private readonly WindowsManager _windowManager;

        [Inject]
        public WeaponTargetMarkerFactory(WindowsManager windowsManager)
        {
            _windowManager = windowsManager;
        }

        public async Awaitable LoadPrefab()
        {
            _prefab = await AssetsManager.LoadComponentAssetDirectly<UIWeaponAimMarker>("ui_weapon_aim_marker");
        }
    
        public UIWeaponAimMarker Create()
        {
            var aimWindow = _windowManager.GetWindow<UIAimWindow>();
            var instance = GameObject.Instantiate(_prefab, aimWindow.MarkersHost);
            return instance;
        }
    }
}
