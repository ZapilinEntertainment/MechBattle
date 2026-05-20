using Scellecs.Morpeh;
using Unity.Mathematics;
using VContainer;
using ZE.MechBattle.Navigation;

namespace ZE.MechBattle
{
    public class MovementTasksFactory
    {
        private readonly EcsTasksFactory _generalTasksFactory;
        private readonly RequestedHexPathsList _requestedHexPathsList;

        [Inject]
        public MovementTasksFactory(EcsTasksFactory ecsTasksFactory, RequestedHexPathsList requestedHexPaths)
        {
            _generalTasksFactory = ecsTasksFactory;
            _requestedHexPathsList = requestedHexPaths;
        }

        public AwaitingToken RequestHexCreation(int2 hexCoord)
        {

        }

        public AwaitingToken RequestHexesCreation(int2 hexCoordA, int2 hexCoordB)
        {
            var tokenA = RequestHexCreation(hexCoordA);
            var tokenB = RequestHexCreation(hexCoordB);
            return _generalTasksFactory.CombineTokens(tokenA, tokenB);
        }

        public AwaitingToken RequestHexPathCalculation(in HexPathSearchRequest request)
        {
            if (_requestedHexPathsList.TryGetRequestToken(request, out var awaitingToken))
                return awaitingToken;
        }

    }
}
