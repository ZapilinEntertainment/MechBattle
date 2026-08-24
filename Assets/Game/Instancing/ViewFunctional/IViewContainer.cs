using Unity.Mathematics;
using UnityEngine;

namespace ZE.MechBattle.Views
{
    public interface IViewContainer
    {
        IView View { get; }
        void OnViewInstanced(IView prefab);
    }
}
