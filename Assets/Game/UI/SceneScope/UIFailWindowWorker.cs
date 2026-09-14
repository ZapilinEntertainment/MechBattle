using R3;
using VContainer;
using ZE.UiService;
using ZE.Workers;

namespace ZE.MechBattle
{
    public class UIFailWindowWorker : Worker
    {
        public Observable<GameFailOption> FailOptionSelectedProperty { get; private set; }
        
        private UIFailWindow _uiWindow;
        private readonly WindowsManager _windowsManager;

        [Inject]
        public UIFailWindowWorker(WindowsManager windowsManager)
        {
            _windowsManager = windowsManager;
        }

        public override void Start()
        {
            base.Start();
            _uiWindow = _windowsManager.ShowWindow<UIFailWindow>();
            FailOptionSelectedProperty = _uiWindow.OptionSelectedCommand;
        }

        public override void Dispose()
        {
            base.Dispose();
            _windowsManager.HideWindow<UIFailWindow>(_uiWindow);
            FailOptionSelectedProperty = null;
        }
    }
}
