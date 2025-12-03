using UnityEngine;
using Unity.Mathematics;

public static class MathExtensions
{
    public static float3 ProjectOnPlane(this float3 vector, float3 planeNormal)
    {
        var dot = math.dot(vector, planeNormal);
        return vector - dot * planeNormal;
    }

    public static RigidTransform ToRigidTransform(this Transform transform) => new(transform.rotation, transform.position);    
    public static float3 GetRightVector(this RigidTransform transform) => math.mul(transform.rot, math.right());    
}
