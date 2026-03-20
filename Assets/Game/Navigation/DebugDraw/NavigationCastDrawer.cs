using UnityEngine;
using Unity.Mathematics;
using TriInspector;

namespace ZE.MechBattle.Navigation.DebugDraw
{
    public class NavigationCastDrawer : MonoBehaviour
    {
        [SerializeField] private NavigationMapDrawer _mapDrawer;
        [SerializeField] private int2 _hexCoord;

        [Button("Cast")]
        private void DoCast()
        {
            if (_mapDrawer.Map == null)
            {
                Debug.LogError("cast map first");
                return;
            }
        }
    }
}
