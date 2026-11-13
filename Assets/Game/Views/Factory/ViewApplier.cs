using Scellecs.Morpeh;
using VContainer;

namespace ZE.MechBattle
{
    public class ViewApplier
    {
        private readonly ViewApplicationStrategiesList _strategiesByType;


        [Inject]
        public ViewApplier(ViewApplicationStrategiesList strategiesList)
        {
            _strategiesByType = strategiesList;
        }

        // can be mono-object or virtual
        public void ApplyView(Entity entity, ViewKey viewKey)
        {
            if (viewKey.Mode == ViewMode.None)
                return;
            _strategiesByType[viewKey.Mode].ApplyView(entity, viewKey.AssetKey);
        }
    }
}
