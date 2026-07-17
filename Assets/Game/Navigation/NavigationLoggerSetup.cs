using System;
using UnityEngine;
using TriInspector;

namespace ZE.MechBattle.Navigation
{
    [Flags]
    public enum NavigationLogEvents
    {
        None = 0,
        FlowMapSet = 1 << 0,
        HexPathSet = 1 << 1,
        FullPortalSelectionLog = 1 << 2,
        PortalsPathBestResult = 1 << 3,
        FlowMapRequest = 1 << 4,
        FlowMapAssigment = 1 << 5,
        TripathProgression = 1 << 6,
        EntityPortalPathStatuses = 1 << 7,
        HexPathCleared = 1 << 8,
        MoveTargetSet = 1 << 9,
    }

    public class NavigationLoggerSetup : MonoBehaviour
    {
        [SerializeField] private NavigationLogEvents _eventSettings;

        private void Awake() => UpdateFlags();

        [Button("Update flags")]
        private void UpdateFlags()
        {
            NavigationLogger.UpdateLogSettings(_eventSettings);
        }
    }

    public static class NavigationLogger
    {
        public static NavigationLogEvents Settings { get; private set; }
        public static void UpdateLogSettings(NavigationLogEvents settings)
        {
            Settings = settings;
        }
    }

    
}
