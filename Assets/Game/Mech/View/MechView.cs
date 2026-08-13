using AYellowpaper.SerializedCollections;
using Unity.Cinemachine;
using UnityEngine;

namespace ZE.MechBattle
{
    public class MechView : SimpleView, IComplexMonoView, ICameraPointView
    {
        [field:SerializeField] public Transform ChassisRoot { get; private set; }
        [field:SerializeField] public LegDataContainer<Transform> LeftLeg { get; private set; }
        [field: SerializeField] public LegDataContainer<Transform> RightLeg { get; private set; }
        [SerializeField] private SerializedDictionary<CameraMode, CinemachineCamera> _cameraPoints;

        public void ActivateVirtualCamera(CameraMode cameraMode)
        {
            if (_cameraPoints.TryGetValue(cameraMode, out var cinCamera))
                cinCamera.enabled = true;
        }

        public bool TryGetPartByKey(ViewPartKey key, out IViewPart viewPart)
        {
            if (key.Index != 0 && key.Index != 1)
            {
                viewPart = null;
                return false;
            } 

            switch(key.Type) 
            {
                case ViewPartType.ChassisRoot:
                    {
                        viewPart = new ViewPartContainer(ChassisRoot);
                        return true;
                    }
                    case ViewPartType.Hip:
                    {
                        viewPart = new ViewPartContainer(key.Index == 1 ? RightLeg.Hip : LeftLeg.Hip);
                        return true;
                    }
                case ViewPartType.Ankle:
                    {
                        viewPart = new ViewPartContainer(key.Index == 1 ? RightLeg.Ankle : LeftLeg.Ankle);
                        return true;
                    }
                case ViewPartType.Foot:
                    {
                        viewPart = new ViewPartContainer(key.Index == 1 ? RightLeg.Foot : LeftLeg.Foot);
                        return true;
                    }
                default:
                    {
                        viewPart = null;
                        return false;
                    }
            }
        }
    }
}
