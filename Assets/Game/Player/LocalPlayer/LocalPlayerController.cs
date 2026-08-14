using Scellecs.Morpeh;
using System;
using UnityEngine;
using VContainer;
using VContainer.Unity;
using ZE.MechBattle.Ecs;

namespace ZE.MechBattle
{
    public class LocalPlayerController : IDisposable
    {
        private readonly IDisposable _subscription;
        private readonly Stash<MechInputComponent> _input;

        [Inject]
        public LocalPlayerController(SceneFlagsManager sceneFlags, World world)
        {
            _subscription = sceneFlags.Subscribe<LocalPlayerViewInstancedFlag>(OnPlayerViewLoaded);
            _input = world.GetStash<MechInputComponent>();
        }

        public void Dispose()
        {
            _subscription.Dispose();
        }

        private void OnPlayerViewLoaded(LocalPlayerViewInstancedFlag flag)
        {
            
        }
    

    }
}
