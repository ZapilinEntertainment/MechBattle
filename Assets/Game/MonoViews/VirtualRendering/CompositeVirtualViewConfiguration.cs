using System;
using System.Collections.Generic;
using Unity.Mathematics;
using UnityEngine;
using ZE.MechBattle.Ecs;

namespace ZE.MechBattle
{
    [CreateAssetMenu(fileName = nameof(CompositeVirtualViewConfiguration), menuName = "Scriptable Objects/" + nameof(CompositeVirtualViewConfiguration))]
    public class CompositeVirtualViewConfiguration : ScriptableObject
    {
        [Serializable]
        public struct VirtualPartConfig
        {
            public ViewPartKey PartKey;
            public VirtualViewComponent ViewData;

            public VirtualPartConfig(VirtualViewComponent viewData)
            {
                PartKey = ViewPartKey.Undefined;
                ViewData = viewData;
            }
        }

        [SerializeField] private VirtualPartConfig[] _configs;
        public IReadOnlyList<VirtualPartConfig> Configs => _configs;
    }
}
