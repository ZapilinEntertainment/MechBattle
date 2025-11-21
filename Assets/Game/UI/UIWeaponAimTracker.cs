using System;
using UnityEngine;
using UnityEngine.UI;
using ZE.MechBattle.Weapons;
using R3;

namespace ZE.MechBattle.UI
{
    // todo: need special contrast shader

    public class UIWeaponAimTracker : MonoBehaviour
    {
        [SerializeField] private Image _markerImage;
        private bool _isTracking = false;
        private TargetData _targetData;
        private Camera _camera;
        private Transform _aimingObject;
        private IDisposable _designatorSubscription;
        private ReactiveProperty<bool> _isMarkerVisibleProperty = new (false);

        
        public void TrackWeapon(Camera camera, MechWeapon weapon)
        {
            _designatorSubscription?.Dispose();
            _camera = camera;
            _aimingObject = weapon.AimingObject;
            _designatorSubscription = weapon.TargetDesignator.TargetDataProperty.Subscribe(x => _targetData = x);
            _isTracking = _aimingObject != null;
        }

        private void Start()
        {
            _isMarkerVisibleProperty.Subscribe(x => _markerImage.enabled = x);
        }

        private void Update()
        {
            var markerIsVisible = false;
            if (_isTracking)
            {
                Vector3 targetPlanePoint;
                if (_targetData.IsDefined)
                    targetPlanePoint = _targetData.Position;
                else
                    targetPlanePoint = _camera.transform.TransformPoint(0f, 0f, GameConstants.AIM_RAY_LENGTH);

                var targetPlane = new Plane(inNormal: _camera.transform.forward, inPoint: targetPlanePoint);
                var ray = new Ray(origin: _aimingObject.position, direction: _aimingObject.forward);
                if (targetPlane.Raycast(ray, out float enter))
                {
                    markerIsVisible = true;
                    var intersection = ray.GetPoint(enter);

                    // idea: do scaling based on distance
                    transform.position = _camera.WorldToScreenPoint(intersection);
                }
            }
            _isMarkerVisibleProperty.Value = markerIsVisible;
        }

        private void OnDestroy()
        {
            _isMarkerVisibleProperty.Dispose();
        }
    }
}
