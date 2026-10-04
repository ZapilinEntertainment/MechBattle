using Scellecs.Morpeh;
using System.Collections.Generic;
using R3;
using System;

namespace ZE.MechBattle
{
    public class CreationResultsList : IDisposable
    {
        public readonly struct CreationRequestData
        {
            public readonly Entity RequestEntity;
            public readonly Entity CreatedEntity;

            public CreationRequestData(Entity request, Entity result)
            {
                RequestEntity = request;
                CreatedEntity = result;
            }
        }

        public Observable<CreationRequestData> OnRequestResultAdded => _requestResultAddedCommand;
        private readonly Dictionary<Entity, Entity> _requestResults;
        private readonly ReactiveCommand<CreationRequestData> _requestResultAddedCommand;
    
        public CreationResultsList()
        {
            _requestResults = new();
            _requestResultAddedCommand = new();
        }

        public void AddResult(Entity requestEntity, Entity requestResult)
        {
            _requestResults.Add(requestEntity, requestResult);
            _requestResultAddedCommand.Execute(new(requestEntity, requestResult));
        }

        public void RemoveResult(Entity requestEntity) => _requestResults.Remove(requestEntity);

        public void Dispose()
        {
            _requestResultAddedCommand.Dispose();
        }
    }
}
