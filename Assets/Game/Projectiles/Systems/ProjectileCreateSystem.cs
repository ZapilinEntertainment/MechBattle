using VContainer;
using Scellecs.Morpeh;
using Unity.IL2CPP.CompilerServices;

namespace ZE.MechBattle.Ecs {
    [Il2CppSetOption(Option.NullChecks, false)]
    [Il2CppSetOption(Option.ArrayBoundsChecks, false)]
    [Il2CppSetOption(Option.DivideByZeroChecks, false)]
    public sealed class ProjectileCreateSystem : EntityCreationSystemBase<ProjectileBuildRequest, ProjectilesFactory> 
    {
        [Inject]
        public ProjectileCreateSystem(ProjectilesFactory factory) : base(factory)
        {
        }

        protected override bool TryExecuteRequest(Entity requestEntity)
        {
            var data = RequestsStash.Get(requestEntity);
            Factory.Build(data.IdKey, data.Point, data.Shooter);
            return true;
        }
    }
}