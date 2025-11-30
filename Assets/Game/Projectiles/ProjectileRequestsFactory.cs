using UnityEngine;
using Unity.Mathematics;
using VContainer;
using Scellecs.Morpeh;
using ZE.MechBattle.Ecs;

namespace ZE.MechBattle
{
    // why not call builder directly - all projectiles wwill be created in same moment of frame
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

        public void CreateProjectileRequest(string id, RigidTransform point, Entity shooter)
        {
            var requestEntity = _world.CreateEntity();
            var idKey = _stringDict.GetStringKey(id);
            //Debug.Log($"registered projectile request: {id} -> {idKey}");
            _requests.Set(requestEntity, new() { Point = point, IdKey = idKey, Shooter = shooter });
        }
    
    }
}
