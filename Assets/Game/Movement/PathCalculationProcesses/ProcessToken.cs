using UnityEngine;

namespace ZE.MechBattle
{
    public interface IProcessToken
    {
        bool IsValid { get; }
        int ProcessIteration { get; }
        int ProcessIndex { get; }
    }

    public struct ProcessToken : IProcessToken
    {
        public bool IsValid { get; }
        public int ProcessIteration { get;}  
        public int ProcessIndex { get;}

        public ProcessToken(int processIndex, int processIteration)
        {
            IsValid = true; 
            ProcessIndex = processIndex;
            ProcessIteration = processIteration;
        }
    
    }
}
