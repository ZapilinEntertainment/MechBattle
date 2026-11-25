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
        private readonly FinalViewFunctionalApplier _finalViewFunctionalApplier;
        private readonly List<IBuildingFeatureApplier> _featureAppliers;
        private readonly IViewContainerHandler _viewContainerHandler;

        private readonly Stash<BlockZoneConfigComponent> _blockConfigs;

        [Inject]
        public BuildingsFactory(
            World world, 
            ViewSynchronizationApplier viewSyncApplier, 
            TransformAspectHandler transformAspectHandler,
            FinalViewFunctionalApplier finalViewFunctionalApplier,
            IEnumerable<IBuildingFeatureApplier> featureAppliers,
            IViewContainerHandler viewContainerHandler)
        {
            _world = world;
            _viewSyncApplier = viewSyncApplier;
            _transformAspectHandler = transformAspectHandler;
            _finalViewFunctionalApplier = finalViewFunctionalApplier;
            _viewContainerHandler = viewContainerHandler;

            
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
            

            _blockConfigs = _world.GetStash<BlockZoneConfigComponent>();
        }

        public Entity CreateEntityBySceneView(IBuildingView view)
        {
            var entity = _world.CreateEntity();
            _transformAspectHandler.InitializeTriangularAndHexPositions(entity, view.Transform.position);

            _viewContainerHandler.RegisterOneShotViewContainer(entity, view);

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
            if (view is IMonoView monoView)
                _finalViewFunctionalApplier.CheckAndApply(entity, monoView);

            return entity;
        }
    

    }
}
