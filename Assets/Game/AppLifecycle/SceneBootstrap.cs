using System;
using ZE.Workers;
using VContainer;
using VContainer.Unity;
using R3;
using ZE.MechBattle.UI;

namespace ZE.MechBattle
{
    public class SceneBootstrap : IStartable, IDisposable
    {
        private readonly CompositeDisposable _lifetimeObject = new();
        private readonly SessionData _sessionData;
        private readonly IObjectResolver _objectResolver;
        private readonly PlayerFactory _playerFactory;

        [Inject]
        public SceneBootstrap(SessionData sessionData, IObjectResolver objectResolver, PlayerFactory playerFactory)
        {
            _sessionData = sessionData;
            _objectResolver = objectResolver;
            _playerFactory = playerFactory;
        }

        public void Start()
        {
            _sessionData.LocalPlayer = _playerFactory.CreateLocalPlayer();

            AddWorker<PlayerInterfaceWorker>();
        }

        public void Dispose()
        {
            _lifetimeObject.Dispose();
        }

        private T AddWorker<T>() where T : Worker
        {
            var worker = _objectResolver.Resolve<T>().AddTo(_lifetimeObject);
            worker.Start();
            return worker;
        }
    }
}
