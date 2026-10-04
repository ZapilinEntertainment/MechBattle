using VContainer;
using Scellecs.Morpeh;
using Unity.IL2CPP.CompilerServices;

namespace ZE.MechBattle.Ecs {
    [Il2CppSetOption(Option.NullChecks, false)]
    [Il2CppSetOption(Option.ArrayBoundsChecks, false)]
    [Il2CppSetOption(Option.DivideByZeroChecks, false)]
    public sealed class ProjectileCreateSystem : RequestHandleSystemBase<ProjectileBuildRequest> 
    {
        private readonly ProjectilesFactory _projectilesFactory;


        [Inject]
        public ProjectileCreateSystem(ProjectilesFactory factory)
        {
            _projectilesFactory = factory;
        }

        protected override bool TryHandleRequest(Entity requestEntity)
        {
            var request = GetRequestComponent(requestEntity);
            _projectilesFactory.Build(request.IdKey, request.Point, request.WeaponEntity, request.ShooterEntity);
            return true;
        }
    }
}