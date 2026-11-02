using Scellecs.Morpeh;
using Unity.Mathematics;

namespace ZE.MechBattle.Ecs
{
    public readonly struct PotentialTargetEntityData
    {
        public readonly int PlayerId;
        public readonly Entity Entity;
        public readonly float3 WorldPos;
        public readonly int2 HexCoord;

        public PotentialTargetEntityData(Entity entity, int playerId, float3 worldPos, int2 hexPos)
        {
            Entity = entity;
            PlayerId = playerId;
            WorldPos = worldPos;
            HexCoord = hexPos;
        }
    }
}
