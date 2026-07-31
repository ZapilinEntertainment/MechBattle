using Scellecs.Morpeh;
using Unity.IL2CPP.CompilerServices;

namespace ZE.MechBattle.Ecs {
    [Il2CppSetOption(Option.NullChecks, false)]
    [Il2CppSetOption(Option.ArrayBoundsChecks, false)]
    [Il2CppSetOption(Option.DivideByZeroChecks, false)]
    public sealed class MechInitializationSystem : ISystem 
    {
        public World World { get; set;}
        private Filter _filter;
        private Stash<MechChassisComponent> _chassisComponents;
        private Stash<MechChassisInitializedTag> _initTags;
        private Stash<ChildTransformLastSyncStampComponent> _transformSyncStamps;
        private Stash<ViewLoadRequestTag> _viewLoadRequests;
        private Stash<ChassisSettingsComponent> _chassisSettings;

        public void OnAwake() 
        {
            _filter = World.Filter
                .With<MechChassisComponent>()
                .Without<MechChassisInitializedTag>()
                .Build();

            _chassisComponents = World.GetStash<MechChassisComponent>();
            _initTags = World.GetStash<MechChassisInitializedTag>();
            _transformSyncStamps = World.GetStash<ChildTransformLastSyncStampComponent>();
            _viewLoadRequests = World.GetStash<ViewLoadRequestTag>();
            _chassisSettings = World.GetStash<ChassisSettingsComponent>();
        }

        public void OnUpdate(float deltaTime) 
        {
            foreach (var chassisEntity in _filter)
            {
                var chassisComponent = _chassisComponents.Get(chassisEntity);
                var chassisReady =
                    CheckIfChassisPartReady(chassisEntity)
                    && CheckIfChassisPartReady(chassisComponent.LeftLeg.Hip)
                    && CheckIfChassisPartReady(chassisComponent.LeftLeg.Ankle)
                    && CheckIfChassisPartReady(chassisComponent.LeftLeg.Foot)
                    && CheckIfChassisPartReady(chassisComponent.RightLeg.Hip)
                    && CheckIfChassisPartReady(chassisComponent.RightLeg.Ankle)
                    && CheckIfChassisPartReady(chassisComponent.RightLeg.Foot);

                if (chassisReady)
                {
                    _initTags.Add(chassisEntity);
                    SyncComponentsCommand.Execute(chassisComponent.LeftLeg.Foot, chassisEntity, _chassisSettings);
                    SyncComponentsCommand.Execute(chassisComponent.RightLeg.Foot, chassisEntity, _chassisSettings);

                    UnityEngine.Debug.Log($"chassis {chassisEntity.Id} initialized");
                }
                    
            }
        }

        public void Dispose() { }

        private bool CheckIfChassisPartReady(Entity partEntity) => !_viewLoadRequests.Has(partEntity) && _transformSyncStamps.Has(partEntity);
    }
}