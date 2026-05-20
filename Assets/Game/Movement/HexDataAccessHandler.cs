using Unity.Mathematics;
using VContainer;
using ZE.MechBattle.Navigation;

namespace ZE.MechBattle
{
    public class HexDataAccessHandler
    {
        private readonly INavigationMap _map;
        private readonly MovementTasksFactory _movementTasks;

        [Inject]
        public HexDataAccessHandler(INavigationMap map, MovementTasksFactory movementTasks)
        {
            _map = map;
            _movementTasks = movementTasks;
        }

        public bool TryGetHexData(int2 hexCoord, out INavigationHex hex, out AwaitingToken awaitingToken, bool requestHexDataCalculation = true)
        {
            var hexExists = _map.TryGetHex(hexCoord, out hex);
            if (hexExists | !requestHexDataCalculation)
            {
                awaitingToken = default;
            }
            else
            {
                awaitingToken = _movementTasks.RequestHexCreation(hexCoord);
            }
            return hexExists;
        }

        public bool TryGetHexData(
            int2 startHexCoord,
            int2 endHexCoord,
            out INavigationHex startHex,
            out INavigationHex endHex,
            out AwaitingToken awaitingToken,
             bool requestHexDataCalculation = true)
        {
            var startHexExists = _map.TryGetHex(startHexCoord, out startHex);
            var endHexExists = _map.TryGetHex(endHexCoord, out endHex);

            var bothHexesPresented = startHexExists & endHexExists;
            if (bothHexesPresented)
            {
                awaitingToken = default;
                return true;
            }               

            if (requestHexDataCalculation) 
            { 
                if (startHexExists | endHexExists)
                    awaitingToken = _movementTasks.RequestHexCreation(endHexExists ? startHexCoord : endHexCoord);
                else
                    awaitingToken = _movementTasks.RequestHexesCreation(startHexCoord, endHexCoord);
            }
            else
            {
                awaitingToken = default;
            }
            return false;
        }

    }
}
