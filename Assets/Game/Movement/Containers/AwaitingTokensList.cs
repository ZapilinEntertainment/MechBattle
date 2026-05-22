using System.Threading.Tasks;
using System.Collections.Generic;

namespace ZE.MechBattle
{
    public class AwaitingTokensList
    {
        private readonly HashSet<AwaitingToken> _actualTokens = new();

        public bool IsAwaitingOver(AwaitingToken token) => !_actualTokens.Contains(token);
        public bool IsTokenActive(AwaitingToken token) => _actualTokens.Contains(token);
    
        public async Task WaitUntilTokenExpires(AwaitingToken token)
        {
            do
            {
                await Task.Yield();
            }
            while (IsTokenActive(token));
        }
    }
}
