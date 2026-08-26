using System;
using TriInspector;

namespace ZE.MechBattle
{
    [Serializable]
    [DeclareFoldoutGroup(SETTINGS, Title = "$" + nameof(Title))]
    public struct MechColliderConfig
    {
        [Group(SETTINGS)] public ViewPartKey Key;
        [Group(SETTINGS)] public MechPartitionKey PartitionKey;
        [Group(SETTINGS)] public ColliderSetupInfo ColliderSetupInfo;

        private const string SETTINGS = "settings";
        private string Title() => $"{PartitionKey} : {Key.Type} : {Key.Index}";
    }
}
