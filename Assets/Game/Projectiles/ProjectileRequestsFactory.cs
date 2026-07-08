using UnityEngine;
using Unity.Mathematics;
using VContainer;
using Scellecs.Morpeh;
using ZE.MechBattle.Ecs;

namespace ZE.MechBattle
{
    // why not call factory directly - all projectiles will be created in same moment of frame
    public class ProjectileRequestsFactory
    {
        private readonly World _world;
        private readonly StringDataDictionary _stringDict;
        private readonly Stash<ProjectileBuildRequest> _requests;
        

        [Inject]
        public ProjectileRequestsFactory(World world, StringDataDictionary stringDict)
        {
            _world = world;
            _stringDict = stringDict;
            _requests = _world.GetStash<ProjectileBuildRequest>();
        }

        public void CreateProjectileRequest(string id, RigidTransform point, Entity shooter) =>
            CreateProjectileRequest(_stringDict.GetStringKey(id), point, shooter);

        public void CreateProjectileRequest(int idKey, RigidTransform point, Entity shooter)
        {
            var requestEntity = _world.CreateEntity();
            _requests.Set(requestEntity, new() { Point = point, IdKey = idKey, Shooter = shooter });
        }
    }
}
