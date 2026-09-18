using UnityEngine;
using ZE.UiService;

namespace ZE.MechBattle.UI
{
    public class MechGameUIRoot : UiRoot
    {

        private void Awake()
        {
            GameObject.DontDestroyOnLoad(gameObject);
        }
    }
}
