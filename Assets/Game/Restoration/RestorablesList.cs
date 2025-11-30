using System;
using System.Collections.Generic;
using VContainer;
using Scellecs.Morpeh;
using ZE.MechBattle.Ecs;
using System.Threading;

namespace ZE.MechBattle
{
    public class RestorablesList : IntEncodingDictionary<IRestorable>, IFlag
    {
        private readonly World _world;
        private readonly Stash<RestorableComponent> _restoreComponents;
        private readonly AppFlagsManager _appFlags;

        [Inject]
        public RestorablesList(World world, AppFlagsManager appFlags)
        {
            _world = world;
            _restoreComponents = _world.GetStash<RestorableComponent>();
            _appFlags = appFlags;
            _appFlags.AddFlag(this);
        }

        public void RegisterRestorable(IRestorable restorable, float restoreTime)
        {
            var key = Register(restorable);

            // direct entity construction - create moment is not important
            // (compared to spawn, no use of request builder) 
            var recordEntity = _world.CreateEntity();
            _restoreComponents.Set(recordEntity, new() { RestoreIndex = key, RestoreTime = restoreTime });
        }

        override public void Dispose()
        {
            _appFlags.RemoveFlag(this);
            foreach (var restorable in Dictionary.Values)
            {
                if (restorable.RestoreIfSessionEnds)
                    restorable.Restore();
            }
            base.Dispose();
        }
    }
}
