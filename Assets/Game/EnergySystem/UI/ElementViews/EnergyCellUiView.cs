using System;
using UnityEngine;
using UnityEngine.UI;

namespace ZE.MechBattle
{
    public class EnergyCellUiView : MonoBehaviour, IPoolableObject<EnergyCellUiView>
    {
        [field:SerializeField] public Color EnergyColor { get; private set; } = Color.cyan;
        [field: SerializeField] public Color RepairColor { get; private set; } = Color.limeGreen;
        [field: SerializeField] public Color HealthColor { get; private set; } = Color.green;
        [Space]
        [SerializeField] private Image _energyLine;
        [SerializeField] private Image _damageLine;
        [SerializeField] private GameObject _repairMarker;
        private bool _isActive = false;
        private IPoolElementReleaser<EnergyCellUiView> _releaser;
        
        public struct UpdateProtocol
        {
            public bool EnableRepairMarker;
            public bool EnableDamageLine;
            public bool HasReceivedDamageOnThisFrame;
            public Color EnergyLineColor;
            public float EnergyLineFillPc;
            public float DamageLineFillPc;
        }

        public void UpdateValues(UpdateProtocol protocol)
        {
            if (!_isActive)
                return;
            _repairMarker.SetActive(protocol.EnableRepairMarker);

            _damageLine.enabled = protocol.EnableDamageLine;
            _damageLine.fillAmount = protocol.DamageLineFillPc;

            _energyLine.fillAmount = protocol.EnergyLineFillPc;
            _energyLine.color = protocol.EnergyLineColor;            

            if (protocol.HasReceivedDamageOnThisFrame)
            {
                // todo: some visual effect
            }
        }

        public void SetupParent(Transform parent) => transform.SetParent(parent, false);

        public void AssignReleaser(IPoolElementReleaser<EnergyCellUiView> releaser) =>
            _releaser = releaser;

        public void Dispose()
        {
            _releaser.Release(this);
            _isActive = false;
        }

        public void OnGet() 
        {
            _isActive = true;
        }

        public void OnRelease() 
        {
            _energyLine.fillAmount = 0f;
            _damageLine.fillAmount = 0f;
            _repairMarker.SetActive(false);
            _isActive = false;
        }

        private void OnDestroy()
        {
            _isActive = false;
        }
    }
}
