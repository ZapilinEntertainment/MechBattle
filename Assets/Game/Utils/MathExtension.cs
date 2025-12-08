using Unity.Burst;
using UnityEngine;
using Unity.Mathematics;
using System.Runtime.CompilerServices;

public static class MathExtensions
{
    public static float3 ProjectOnPlane(this float3 vector, float3 planeNormal)
    {
        var dot = math.dot(vector, planeNormal);
        return vector - dot * planeNormal;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static RigidTransform ToRigidTransform(this Transform transform) => new(transform.rotation, transform.position);

    [BurstCompile]
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static float3 GetRightVector(in RigidTransform transform) => math.mul(transform.rot, math.right());

    [BurstCompile]
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static quaternion RotateTowards(in quaternion from, in quaternion to, float angleInDegrees)
    {
        var maxAngle = math.angle(from, to) * math.TODEGREES;
        if (maxAngle <= math.EPSILON)
        {
            return to;
        }

        return math.slerp(from, to, math.min(1f, angleInDegrees / maxAngle));
    }
}
