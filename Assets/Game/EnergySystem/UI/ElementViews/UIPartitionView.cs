using Unity.Mathematics;
using UnityEngine;

namespace ZE.MechBattle
{
    public class UIPartitionView : MonoBehaviour
    {
        public enum PartitionViewMode : byte
        {
            HealthDisplay,
            Repairs,
            CellsCharging
        }

        [SerializeField] private UIQuadGridRenderer _damagePanelGrid;
        [SerializeField] private GameObject _repairIcon;
        [SerializeField] private GameObject _cellsPanel;
        public Transform CellsHost => _cellsPanel.transform;


        public void SwitchMode(PartitionViewMode viewMode)
        {
            _damagePanelGrid.enabled = viewMode != PartitionViewMode.CellsCharging;
            _repairIcon.SetActive(viewMode == PartitionViewMode.Repairs);
            _cellsPanel.SetActive(viewMode == PartitionViewMode.CellsCharging);

            if (viewMode == PartitionViewMode.CellsCharging) 
                _damagePanelGrid.Seed = (int)math.lerp(int.MinValue, int.MaxValue, UnityEngine.Random.value);
        }

        public void SetHealthPc(float pc) => _damagePanelGrid.Value = pc;
    }
}
