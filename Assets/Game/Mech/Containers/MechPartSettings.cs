using System;
using System.Collections.Generic;
using Unity.Mathematics;
using UnityEngine;
using TriInspector;

namespace ZE.MechBattle
{
    [Serializable]
    [DeclareFoldoutGroup(SETTINGS, Title ="$" + nameof(Title))]
    
    public struct MechPartSettings
    {
        [Group(SETTINGS)] public ViewPartKey Key;
        [Space]
        [Group(SETTINGS)] public MechPartConstructionMode ConstructionMode;
        [Group(SETTINGS)] public ViewPartKey RootKey;
        [Group(SETTINGS)] public MechPartitionKey Partition;
        [Space]
        [Group(SETTINGS)] public ViewPartAttachmentProtocol AttachProtocol;
        [Space]
        [Group(SETTINGS)] public float RotationSpeedDegrees;
        [Group(SETTINGS)] public ForwardRotationLimits RotationLimits;
        [Space]
        [Group(SETTINGS)] public List<string> SpecialKeywords;

        [Group(SETTINGS)] public float RotationSpeedRadians => math.radians(RotationSpeedDegrees);

        private string Title() => $"{Key.Type} : {Key.Index}";

        private const string SETTINGS = "settings";
    }
}
