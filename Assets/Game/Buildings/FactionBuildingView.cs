using UnityEngine;

namespace ZE.MechBattle
{
    public class FactionBuildingView : BuildingView, IFactionableView
    {
        [SerializeField] private Renderer[] _colouredRenderers;

        public void ApplyFactionMaterial(Material material)
        {
            foreach (var renderer in _colouredRenderers)
                renderer.sharedMaterial = material;
        }
    }
}
