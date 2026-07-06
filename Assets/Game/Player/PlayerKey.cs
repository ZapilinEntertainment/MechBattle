using System;
namespace ZE.MechBattle
{
    [Serializable]
    public struct PlayerKey
    {
        public int Id;

        public PlayerKey(int id)
        {
            Id = id;
        }
    
    }
}
