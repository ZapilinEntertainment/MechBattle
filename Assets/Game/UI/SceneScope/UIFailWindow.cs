using UnityEngine;
using ZE.UiService;
using R3;

namespace ZE.MechBattle
{
    public class UIFailWindow : UiWindow
    {
        public Observable<GameFailOption> OptionSelectedCommand => _optionSelectedCommand;
        private ReactiveCommand<GameFailOption> _optionSelectedCommand = new();

        public void BUTTON_Restart() => _optionSelectedCommand.Execute(GameFailOption.Restart);

        private void OnDestroy()
        {
            _optionSelectedCommand.Dispose();
        }
    }
}
