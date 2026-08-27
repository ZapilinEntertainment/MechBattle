using System;

namespace ZE.MechBattle
{
    public struct IncomingDamageData
    {
        public float Volume;
        public ReceivedDamageFlag Flags;

        public IncomingDamageData Add(IncomingDamageData data) => new() { 
            Volume = this.Volume + data.Volume, 
            Flags = this.Flags | data.Flags
        };
    }

    [Flags]
    public enum ReceivedDamageFlag : byte { 
        None = 0, 
        Trampled = 1 << 0
    }
}
