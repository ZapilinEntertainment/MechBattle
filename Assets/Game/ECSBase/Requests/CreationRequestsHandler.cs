using Scellecs.Morpeh;
using System;
using System.Threading;
using UnityEngine;
using VContainer;
using ZE.MechBattle.Ecs;
using R3;

namespace ZE.MechBattle
{
    public class CreationRequestsHandler : IDisposable
    {
        private readonly Stash<CreationRequestResultComponent> _results;
        private readonly CreationResultsList _resultsList;
        private readonly CancellationTokenSource _cancellationTokenSource;

        [Inject]
        public CreationRequestsHandler(World world, CreationResultsList resultsList)
        {
            _resultsList = resultsList;
            _results = world.GetStash<CreationRequestResultComponent>();
            _cancellationTokenSource = new();
        }

        public async Awaitable<Entity> WaitUntilRequestCompleted(Entity requestEntity, CancellationToken cancellationToken)
        {
            _results.Set(requestEntity);
            using var operationCts = CancellationTokenSource.CreateLinkedTokenSource(_cancellationTokenSource.Token, cancellationToken);
            var resultingData = await _resultsList.OnRequestResultAdded
                .FirstAsync(
                    resultingData => resultingData.RequestEntity == requestEntity,
                    operationCts.Token);

            _resultsList.RemoveResult(resultingData.RequestEntity);

            return resultingData.CreatedEntity;
        }

        public void Dispose()
        {
            _cancellationTokenSource.Cancel();
            _cancellationTokenSource.Dispose();
        }
    
    }
}
