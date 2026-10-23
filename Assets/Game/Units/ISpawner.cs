using Unity.Mathematics;
using Scellecs.Morpeh;
using ZE.MechBattle.Ecs;

namespace ZE.MechBattle
{
    public interface ISpawner
    {
        float InitialDelay { get; }
        float UpdateIntervalDuration { get; }
        float3 WorldPos { get; }
        PlayerKey PlayerKey { get; }

        SpawnerComponent GetSpawnerData(in StringDataDictionary stringDataDictionary);

        bool TryGetLimit(out int limit);
    
    }
}
