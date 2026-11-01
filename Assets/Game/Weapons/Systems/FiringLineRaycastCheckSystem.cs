using Scellecs.Morpeh;
using Unity.IL2CPP.CompilerServices;
using UnityEngine;
using Unity.Mathematics;
using VContainer;

namespace ZE.MechBattle.Ecs {
    [Il2CppSetOption(Option.NullChecks, false)]
    [Il2CppSetOption(Option.ArrayBoundsChecks, false)]
    [Il2CppSetOption(Option.DivideByZeroChecks, false)]
    public sealed class FiringLineRaycastCheckSystem : FiringLineCheckSystemBase<CalculateFireLineByRaycastTag> 
    {
        [Inject]
        public FiringLineRaycastCheckSystem(CollidersTable collidersTable, AffinityHandler affinityHandler, SceneFlagsManager flagsManager) : base(collidersTable, affinityHandler, flagsManager)
        {
        }    

        protected override void PrepareAndLaunchJob()
        {
            foreach (var entityCastData in _activeJobEntities)
            {
                var shotPoint = _weaponShotPoints.Get(entityCastData.WeaponEntity).WorldPoint;
                var range = _weaponRanges.Get(entityCastData.WeaponEntity).MaxRange;

                _commandsList.Add(new RaycastCommand(from: shotPoint.pos, direction: math.forward(shotPoint.rot), _queryParameters, range));
            }
            _resultsList.ResizeUninitialized(_activeJobEntities.Count);

            // Schedule the batch of raycasts.
            _activeJobHandle = RaycastCommand.ScheduleBatch(_commandsList.AsArray(), _resultsList.AsArray(), minCommandsPerJob: 64, maxHits: 1);
            _isJobActive = true;
        }

        override protected void ApplyPreviousJob()
        {
            _activeJobHandle.Complete();
            for (var i = 0; i < _activeJobEntities.Count; i++)
            {
                var firingLineIsClear = true;
                var entityCastData = _activeJobEntities[i];

                if (World.IsDisposed(entityCastData.WeaponEntity))
                    continue;

                if (World.IsDisposed(entityCastData.TargetEntity))
                {
                    firingLineIsClear = false;
                }                    
                else
                {
                    firingLineIsClear = IsFirelineClear(i, entityCastData.WeaponEntity, entityCastData.TargetEntity);
                }               

                if (firingLineIsClear)
                    _fireLineClearTag.Set(entityCastData.WeaponEntity);
                else
                    _fireLineClearTag.Remove(entityCastData.WeaponEntity);
            }
        }

        
    }
}