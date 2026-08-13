using UnityEngine;

namespace ZE.MechBattle
{
    public interface ICameraPointView : IView
    {
        void ActivateVirtualCamera(CameraMode cameraMode);
    
    }
}
