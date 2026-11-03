using UnityEngine;
using System.Collections.Generic;
using System;
using R3;
using UnityEditor;

namespace ZE.MechBattle.Develop
{

    public class RealtimeRaycastCommandsDrawer : MonoBehaviour
    {
        private readonly List<RaycastCommand> _raycastCommands = new();
        private IDisposable _subscription;

        private void Awake()
        {
            _subscription = MessageBroker.Receive<RaycastCommand>().Subscribe(OnMessageReceived);
        }

        private void OnMessageReceived(RaycastCommand raycastCommand)
        {
            _raycastCommands.Add(raycastCommand);
        }

        private void OnDestroy()
        {
            _subscription.Dispose();
        }

        private void OnDrawGizmos()
        {
            if (!isActiveAndEnabled)
                return;

            Handles.color = Color.yellowNice;
            foreach (var raycastCommand in _raycastCommands)
            {
                Handles.DrawLine(raycastCommand.from, raycastCommand.from + raycastCommand.distance * raycastCommand.direction);
            }
            _raycastCommands.Clear();
        }
    }
}
