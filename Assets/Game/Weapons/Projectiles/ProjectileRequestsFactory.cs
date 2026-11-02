using Unity.Mathematics;
using VContainer;
using Scellecs.Morpeh;
using ZE.MechBattle.Ecs;

namespace ZE.MechBattle
{
    // why not call factory directly - all projectiles will be created in same moment of frame
    public class ProjectileRequestsFactory : RequestFactoryBase<ProjectileBuildRequest>
    {
        private readonly StringDataDictionary _stringDict;
        private readonly AffinityHandler _affinityHandler;
        

        [Inject]
        public ProjectileRequestsFactory(World world, StringDataDictionary stringDict, AffinityHandler affinityHandler) : base(world) 
        {
            _stringDict = stringDict;
            _affinityHandler = affinityHandler;
        }

        public void CreateProjectileRequestByStringKey(string id, RigidTransform point, Entity weaponEntity)
        {
            var idKey = _stringDict.StringToKey(id);
            CreateProjectileRequestById(idKey, point, weaponEntity);
        }
            

        public void CreateProjectileRequestById(int idKey, RigidTransform point, Entity weaponEntity) =>
            CreateRequest(new() { Point = point, IdKey = idKey, WeaponEntity = weaponEntity, ShooterEntity = _affinityHandler.GetEntityOwner(weaponEntity)});
    }
}
