using UnityEngine;
using ZE.MechBattle.Views;

namespace ZE.MechBattle
{
    public class ViewInstanceProvider : IViewProvider
    {
        public bool IsReadyToProvide => true;
        private readonly SimpleView _prefab;

        public ViewInstanceProvider(SimpleView view)
        {
            _prefab = view;
        }

        public void Dispose() { }

        public IView GetView() => GameObject.Instantiate(_prefab);
    }
}
