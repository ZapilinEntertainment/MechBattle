using System;
using UnityEngine;

namespace ZE.MechBattle.Views
{
    [CreateAssetMenu(fileName = nameof(ComplexVirtualViewConfigurationObject), menuName = "Scriptable Objects/" + nameof(ComplexVirtualViewConfigurationObject))]
    public class ComplexVirtualViewConfigurationObject : ScriptableObject
    {
        [Serializable]
        public class CompositeObjectViewPartData
        {
            public ViewPartKey PartKey;
            public ViewPartKey ParentKey;
            public VirtualViewSetupData ViewData;
        }

        [field: SerializeField] public CompositeObjectViewPartData[] ViewPartsData { get; private set; }

        public bool TryGetDataByKey(ViewPartKey key, out CompositeObjectViewPartData data)
        {
            foreach (var viewPartData in ViewPartsData)
            {
                if (viewPartData.PartKey == key)
                {
                    data = viewPartData;
                    return true;
                }
            }

            data = default;
            return false;
        }

    }
}
