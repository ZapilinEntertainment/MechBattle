using System;
using UnityEngine;
using R3;

namespace ZE.MechBattle.Weapons
{
    public abstract class MechWeapon : MonoBehaviour
    {
        public abstract float AimSpeed { get; }
        public abstract bool ShowInterfaceAim { get; }
        public abstract Transform AimingObject { get; }
        public ITargetDesignator TargetDesignator { get; private set; }
        private TargetData _targetData;
        private IDisposable _designatorSubscription;


        public virtual void SetDesignator(ITargetDesignator designator)
        {
            _designatorSubscription?.Dispose();

            TargetDesignator = designator;
            _designatorSubscription = TargetDesignator
                .TargetDataProperty
                .Subscribe(data => _targetData = data);
        }

        public abstract void Fire();

        private void Update()
        {
            if (!_targetData.IsDefined)
                return;
            var targetRotation = Quaternion.LookRotation((_targetData.Position - AimingObject.position).normalized, transform.up);
            AimingObject.rotation = Quaternion.RotateTowards(AimingObject.rotation, targetRotation, AimSpeed * Time.deltaTime);
        }

        private void OnDestroy()
        {
            _designatorSubscription?.Dispose();
        }
    }
}
