using UnityEngine;

namespace ZE.MechBattle
{
    public class UIWeaponAimMarker : DisposableGameObject
    {
        public void SetVisibility(bool isVisible) => gameObject.SetActive(isVisible);

        public void SetPosition(Vector3 screenPos) => transform.position = screenPos;
    }
}
