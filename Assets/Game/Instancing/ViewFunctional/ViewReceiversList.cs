using System;
using System.Threading;
using System.Collections.Generic;

namespace ZE.MechBattle.Views
{
    public class ViewReceiversList : IntEncodingDictionary<IViewLoadReceiver>
    {
        public override bool TryGetElement(int key, out IViewLoadReceiver value)
        {
            if (! base.TryGetElement(key, out value))
                return false;

            if (value.IsDisposed)
            {
                Unregister(key);
                return false;
            }
            return true;
        }
    }
}
