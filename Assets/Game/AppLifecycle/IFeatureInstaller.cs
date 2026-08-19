using System.IO;
using UnityEngine;
using VContainer;
using ZE.MechBattle;

public interface IFeatureInstaller
{
    static void RegisterScriptable<T>(IContainerBuilder builder) where T : ScriptableObject
    {
        var typeString = typeof(T).Name;
        var scriptable = Resources.Load<T>(Path.Combine(DirectoryConstants.SCRIPTABLES_FOLDER, typeString));
        if (scriptable == null)
            Debug.LogError($"{DirectoryConstants.SCRIPTABLES_FOLDER} {typeString} not found");
        else
            builder.RegisterInstance(scriptable);
    }
}

public interface IAppFeatureScopeInstaller : IFeatureInstaller
{
    void AppScopeInstall(IContainerBuilder builder);
}

public interface ISessionFeatureScopeInstaller : IFeatureInstaller
{
    void SessionScopeInstall(IContainerBuilder builder);
}
public interface ISceneFeatureScopeInstaller : IFeatureInstaller
{
    void SceneScopeInstall(IContainerBuilder builder);
}
