using System;
using UnityEngine;
using VContainer;

namespace ZE.MechBattle
{
    public class TimeController : IDisposable
    {
        private IDisposable _subscription;

        [Inject]
        public TimeController(SceneFlagsManager sceneFlagsManager)
        {
            _subscription = sceneFlagsManager.Subscribe<PauseFlag>(OnPauseFlagChanged);
        }

        public void Dispose()
        {
            _subscription.Dispose();
            OnPauseFlagChanged(false);
        }
    
        private void OnPauseFlagChanged(bool isPaused)
        {
            Time.timeScale = isPaused ? 0f : 1f;
        }        
    }
}
