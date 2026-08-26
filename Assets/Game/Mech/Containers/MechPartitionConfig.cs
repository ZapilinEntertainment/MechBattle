using System;
using TriInspector;

namespace ZE.MechBattle
{
    [Serializable]
    [DeclareFoldoutGroup(SETTINGS, Title = "$" + nameof(Title))]
    public struct MechPartitionConfig
    {
        [Group(SETTINGS)] public MechPartitionKey Key;
        [Group(SETTINGS)] public ViewPartKey RootPartKey;
        [Group(SETTINGS)] public ViewPartAttachmentProtocol AttachProtocol;

        private const string SETTINGS = "settings";
        private string Title() => $"{Key.Type} : {Key.Index}";
    }
}
