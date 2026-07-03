using UnityEngine;
using ZE.MechBattle.Views;

namespace ZE.MechBattle
{
    public class SimpleView : DisposableGameObject, IView
    {

        public override void SetParent(Transform parent) 
        {
            transform.SetParent(parent, false);
            transform.SetLocalPositionAndRotation(Vector3.zero, Quaternion.identity);
        }
    }
}
