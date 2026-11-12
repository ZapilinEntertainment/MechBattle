using System;
using TriInspector;
using Unity.Mathematics;
using UnityEngine;

namespace ZE.MechBattle.Views
{
    [Serializable]
    public class VirtualViewSetupData
    {
        [field: SerializeField] public float3 Position { get; private set; }
        [SerializeField] private float3 _eulerAngles;
        public Quaternion Rotation => Quaternion.Euler(math.radians(_eulerAngles));

        [PropertySpace]
        [field: SerializeField] public string MeshKey { get; private set; }
        [field: SerializeField] public int SubmeshIndex { get; private set; } = 0;
        [field: SerializeField] public string MaterialKey { get; private set; }
        [field: SerializeField] public float3 Scale { get; private set; } = new float3(1f);
        [field: SerializeField] public float VisibilityLimit { get; private set; } = 0f;

    }
}
