using System;

namespace ZE.MechBattle
{
    public struct LoadingToken
    {
        public bool IsCompleted;    
        public Guid ProcessGuid;

        public static LoadingToken Completed = new() { IsCompleted = true, ProcessGuid = Guid.Empty };
        public static LoadingToken Cancelled = new() { IsCompleted = false, ProcessGuid = Guid.Empty};
    }
}
