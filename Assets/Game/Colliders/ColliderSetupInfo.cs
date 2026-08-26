using System;
using Unity.Mathematics;
using UnityEngine;
using UnityEditor;

namespace ZE.MechBattle
{
    [Serializable]
    public struct ColliderSetupInfo
    {
        public ColliderType ColliderType;
        public float3 LocalPosition;
        public quaternion LocalRotation;
        public float3 Size;

        public ColliderSetupInfo(BoxCollider boxCollider)
        {
            ColliderType = ColliderType.Box;
            LocalPosition = boxCollider.center;
            LocalRotation = quaternion.identity;
            Size = boxCollider.size;
        }

        public ColliderSetupInfo(SphereCollider sphereCollider)
        {
            ColliderType = ColliderType.Sphere;
            LocalPosition = sphereCollider.center;
            LocalRotation = quaternion.identity;
            Size = sphereCollider.radius * new float3(1,1,1);
        }
    }

    public enum ColliderType : byte
    {
        Box, Sphere
    }
}
