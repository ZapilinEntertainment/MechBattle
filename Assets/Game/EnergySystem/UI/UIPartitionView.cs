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
    

        public void SwitchMode(PartitionViewMode viewMode)
        {
            _damagePanelGrid.enabled = viewMode != PartitionViewMode.CellsCharging;
            _repairIcon.SetActive(viewMode == PartitionViewMode.Repairs);
            _cellsPanel.SetActive(viewMode == PartitionViewMode.CellsCharging);
        }
    }
}
