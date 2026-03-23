using System;
using UnityEngine;

namespace ZE.MechBattle.Navigation.DebugOverlay
{
    public static class NavigationDebugDataContainer
    {
        public static INavigationMap Map { get; private set; }
        public static INavigationCaster Caster { get; private set; }
        public static MapSettingsSO MapSettings { get; private set; }
        public static event Action<INavigationMap> MapUpdatedEvent;
        public static event Action<MapSettingsSO> MapSettingsChangedEvent;

        public static void SetMap(NavigationMap map)
        {
            Map = map;
            MapUpdatedEvent?.Invoke(map);
        }

        public static void SetCaster(NavigationCaster caster)
        {
            Caster = caster;
        }

        public static void SetMapSettings(MapSettingsSO mapSettings)
        {
            MapSettings = mapSettings;
            MapSettingsChangedEvent?.Invoke(mapSettings);
        }
    }
}
