using System.Collections;
using System.Collections.Generic;

namespace ZE.MechBattle.Damage
{
    public class DamageRequestsList : IEnumerable<DamageApplyRequest>
    {
        public bool IsEmpty => _requests.Count == 0;
        private readonly List<DamageApplyRequest> _requests = new();


        public void Add(DamageApplyRequest request) => _requests.Add(request);
        public void Clear() => _requests.Clear();

        public IEnumerator<DamageApplyRequest> GetEnumerator()
        {
            return ((IEnumerable<DamageApplyRequest>)_requests).GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return ((IEnumerable)_requests).GetEnumerator();
        }
    }
}
