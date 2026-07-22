using UnityEngine;
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
        

        [Inject]
        public ProjectileRequestsFactory(World world, StringDataDictionary stringDict) : base(world) 
        {
            _stringDict = stringDict;
        }

        public void CreateProjectileRequestById(string id, RigidTransform point, Entity shooter)
        {
            var idKey = _stringDict.StringToKey(id);
            CreateProjectileRequestByKey(idKey, point, shooter);
        }
            

        public void CreateProjectileRequestByKey(int idKey, RigidTransform point, Entity shooter) =>
            CreateRequest(new() { Point = point, IdKey = idKey, Shooter = shooter });
    }
}
