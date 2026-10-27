using Unity.Mathematics;
using ZE.MechBattle.Ecs;

namespace ZE.MechBattle
{
    public interface ISpawner
    {
        float InitialDelay { get; }
        float UpdateIntervalDuration { get; }
        float3 WorldPos { get; }

        SpawnerComponent GetSpawnerData(in StringDataDictionary stringDataDictionary);

        bool TryGetLimit(out int limit);
    
    }
}
