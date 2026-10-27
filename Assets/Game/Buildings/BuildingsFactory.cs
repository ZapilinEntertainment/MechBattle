using System.Collections.Generic;
using Scellecs.Morpeh;
using VContainer;
using ZE.MechBattle.Ecs;

namespace ZE.MechBattle
{
    public class BuildingsFactory
    {
        private readonly World _world;
        private readonly ViewSynchronizationApplier _viewSyncApplier;
        private readonly TransformAspectHandler _transformAspectHandler;
        private readonly List<IBuildingFeatureApplier> _featureAppliers;

       
        private readonly Stash<DisposableViewComponent> _disposableView;
        private readonly Stash<BlockZoneConfigComponent> _blockConfigs;

        [Inject]
        public BuildingsFactory(
            World world, 
            ViewSynchronizationApplier viewSyncApplier, 
            TransformAspectHandler transformAspectHandler,
            IEnumerable<IBuildingFeatureApplier> featureAppliers)
        {
            _world = world;
            _viewSyncApplier = viewSyncApplier;
            _transformAspectHandler = transformAspectHandler;

            
            if (featureAppliers != null)
            {
                _featureAppliers = new(4);
                foreach (var featureApplier in featureAppliers)
                {
                    _featureAppliers.Add(featureApplier);
                }
            }
            else
            {
                _featureAppliers = new(0);
            }
            

            _disposableView = _world.GetStash<DisposableViewComponent>();
            _blockConfigs = _world.GetStash<BlockZoneConfigComponent>();
        }

        public Entity CreateEntityBySceneView(IBuildingView view)
        {
            var entity = _world.CreateEntity();
            _transformAspectHandler.InitializeTriangularAndHexPositions(entity, view.Transform.position);
            _disposableView.Add(entity, new(view));

            if (view.TryGetBlockZoneConfig(out var blockComponent))
                _blockConfigs.Add(entity, blockComponent);

            var functionalFlags = view.FunctionFlags;
            foreach (var applier in _featureAppliers)
            {                
                if ((applier.RequiredFlagsMask & view.FunctionFlags) == applier.RequiredFlagsMask)
                    applier.ApplyBuildingFeature(entity, view);
            }          

            // should be last for correct entity components check
            _viewSyncApplier.Apply(entity, view, applyViewPosition: true);

            return entity;
        }
    

    }
}
