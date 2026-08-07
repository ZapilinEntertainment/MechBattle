using Scellecs.Morpeh;
using Unity.Mathematics;
using UnityEngine;
using VContainer;
using ZE.MechBattle.Ecs;
using ZE.MechBattle.MechMovement;

namespace ZE.MechBattle
{
    public class StepDrawer
    {
        private readonly MechMovementHandler _mechHandler;
        private readonly TransformAspectHandler _transformHandler;
        private readonly Stash<StepTargetPointComponent> _targets;

        [Inject]
        public StepDrawer(MechMovementHandler mechMovementHandler, World world, TransformAspectHandler transformAspectHandler)
        {
            _mechHandler = mechMovementHandler;
            _transformHandler = transformAspectHandler;
            _targets = world.GetStash<StepTargetPointComponent>();
        }

        public void DrawStep(Entity chassisEntity)
        {
            var foots = _mechHandler.GetFoots(chassisEntity);
            var backFootPoint = _transformHandler.GetPoint(foots.backFoot);
            var activeFootPoint = _transformHandler.GetPoint(foots.activeFoot);

            var barycenter = math.lerp(activeFootPoint.pos, backFootPoint.pos, 0.5f);

            var hostGO = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            hostGO.name = "step " + Time.frameCount.ToString();
            var host = hostGO.transform;
            host.position = barycenter;
            host.rotation = _transformHandler.GetRotation(chassisEntity);

            var backFootGO = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            backFootGO.name = "backFoot";
            var backFootMark = backFootGO.transform;
            backFootMark.parent = host;
            
            backFootMark.position = backFootPoint.pos;
            backFootMark.rotation = backFootPoint.rot;

            var activeFootGO = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            activeFootGO.name = "activeFoot";
            var activeFootMark = activeFootGO.transform;
            activeFootMark.parent = host;           
            activeFootMark.position = activeFootPoint.pos;
            activeFootMark.rotation = activeFootPoint.rot;

            var targetMarkGO = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            targetMarkGO.name = "target mark";
            var targetMark = targetMarkGO.transform;
            targetMark.parent = host;
            var targetPoint = _targets.Get(foots.activeFoot).Value;
            targetMark.position = targetPoint.pos;
            targetMark.rotation = targetPoint.rot;
        }    
    }
}
