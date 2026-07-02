using UnityEngine;
using Unity.Mathematics;
using Scellecs.Morpeh;
using ZE.MechBattle.Ecs;

namespace ZE.MechBattle
{
    public interface ISpawner
    {
        Entity Entity { get; }
        float InitialDelay { get; }
        float UpdateIntervalDuration { get; }
        float3 WorldPos { get; }
        PlayerKey PlayerKey { get; }

        void OnRegistered(Entity entity, ISpawnersManager manager);
        SpawnerComponent GetSpawnerData();
    
    }
}
