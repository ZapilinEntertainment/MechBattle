using UnityEngine;
using TriInspector;
using ZE.MechBattle.Navigation;
using Unity.Mathematics;
using VContainer;

namespace ZE.MechBattle.Develop
{
    public class HexConnectionChecker : MonoBehaviour
    {
        [SerializeField] private int2 _startHexCoord;
        [SerializeField] private HexEdge _startEdge;
        [SerializeField] private int2 _endHexCoord;
        [SerializeField] private HexEdge _endEdge;
        private INavigationMap _map;
        [ReadOnly, SerializeField] private string _checkResult;

        [Inject]
        public void Inject(INavigationMap map)
        {
            _map = map;
        } 

        [EnableInPlayMode, Button("Check")]
        private void Check()
        {
            if (HexTransitionLogic.IsEdgeTransitionPossible(_startHexCoord, _endHexCoord, _startEdge, _endEdge, _map))
                _checkResult = "transition possible";
            else
                _checkResult = "transition not possible";
        }
    }
}
