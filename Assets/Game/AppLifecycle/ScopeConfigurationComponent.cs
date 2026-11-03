using UnityEngine;
using VContainer;

namespace ZE.MechBattle
{
    public abstract class ScopeConfigurationComponent : MonoBehaviour
    {
        public abstract void OnConfigure(IContainerBuilder builder);
    
    }
}
