using UnityEngine;
using VContainer;
using TriInspector;
using R3;
using Scellecs.Morpeh;
using ZE.MechBattle.Ecs;

namespace ZE.MechBattle.Develop
{
    public class DevelopMechAffector : MonoBehaviour
    {
        private SceneFlagsManager _sceneFlags;
        private CompositeDisposable _compositeDisposable = new();
        private Entity _mechEntity;
        private World _world;
        private Stash<EntityDisposeTag> _disposedStash;

        [Inject]
        public void Inject(SceneFlagsManager sceneFlags, World world)
        {
            _sceneFlags = sceneFlags;
            _world = world;

            _disposedStash = _world.GetStash<EntityDisposeTag>();

            _sceneFlags
                .Subscribe<LocalPlayerVehicleAssignedFlag>(flag => _mechEntity = flag.VehicleEntity)
                .AddTo(_compositeDisposable);
        }

        [Button]
        public void DestroyMech()
        {
            if (_world.IsDisposed(_mechEntity))
                return;

            _disposedStash.Set(_mechEntity);
        }

        private void OnDestroy()
        {
            _compositeDisposable.Dispose();
        }
    }
}
