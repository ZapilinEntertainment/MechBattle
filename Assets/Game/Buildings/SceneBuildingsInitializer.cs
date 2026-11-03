using UnityEngine;
using VContainer;
using VContainer.Unity;

namespace ZE.MechBattle.Ecs {

    // create damageable entities from scene decorations
    // (using searching to avoid direct inject in every damageable decoration GO)
    public sealed class SceneBuildingsInitializer : IInitializable
    {
        private readonly BuildingsFactory _buildingsFactory;

        [Inject]
        public SceneBuildingsInitializer(BuildingsFactory buildingsFactory)
        {
            _buildingsFactory = buildingsFactory;
        }

        public void Initialize()
        {
            var count = 0;
            var destructibleDecorations = GameObject.FindObjectsByType<BuildingView>(FindObjectsSortMode.None);
            foreach (var decoration in destructibleDecorations)
            {
                _buildingsFactory.CreateEntityBySceneView(decoration);
                count++;
            }
        }

     
    }
}