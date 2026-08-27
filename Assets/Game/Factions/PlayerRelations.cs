using Unity.Mathematics;
using Unity.Collections;

namespace ZE.MechBattle
{
    public class PlayerRelations
    {
        // there can be more complex relations encoded

        public PlayerRelationsMask GetEnemiesMask(PlayerKey playerKey) => GetEnemiesMask(playerKey.Id);   

        public PlayerRelationsMask GetEnemiesMask(int playerId)
        {
            var mask = new BitField32(int.MaxValue);
            mask.SetBits(playerId, false);
            return new(mask);
        }

        public bool AreHostile(PlayerKey playerA, PlayerKey playerB)
        {
            var playerAMask = GetEnemiesMask(playerA);
            return playerAMask.Contains(playerB);
        }
    }

    public readonly struct PlayerRelationsMask
    {
        private readonly BitField32 _mask;

        public PlayerRelationsMask(BitField32 mask) => _mask = mask;

        public bool Contains(PlayerKey playerKey) => _mask.IsSet(playerKey.Id);
    }
}
