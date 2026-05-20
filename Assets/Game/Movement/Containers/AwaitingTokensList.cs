using System.Collections.Generic;

namespace ZE.MechBattle
{
    public class AwaitingTokensList
    {
        private readonly HashSet<AwaitingToken> _actualTokens = new();

        public bool IsAwaitingOver(AwaitingToken token) => !_actualTokens.Contains(token);
        public bool IsTokenActive(AwaitingToken token) => _actualTokens.Contains(token);
    
    }
}
