using UnityEngine;

namespace ZE.MechBattle
{
    public readonly struct DrawPointMessage
    {
        public readonly Vector3 Pos;
        public readonly string Text;

        public DrawPointMessage(Vector3 pos, string text)
        {
            Pos = pos;
            Text = text;
        }
    }
}
