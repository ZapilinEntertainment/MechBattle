using System.Collections.Generic;
using System.IO;
using System.Threading;
using UnityEngine;
using VContainer;
using VContainer.Unity;
using ZE.MechBattle.UI;
using ZE.UiService;

namespace ZE.MechBattle
{
    public class UIFeatureModule : IFeatureModule, IAppFeatureScopeInstaller, ISessionAsyncResourceLoader, ISceneFeatureInitializer
    {
        public void AppScopeInstall(IContainerBuilder builder)
        {
            var uiRootPrefab = Resources.Load<MechGameUIRoot>("UIRoot");
            builder.RegisterComponentInNewPrefab(uiRootPrefab, Lifetime.Singleton).As<UiRoot>();
            builder.Register<WindowsManager>(Lifetime.Singleton);

            builder.Register<WeaponTargetMarkerFactory>(Lifetime.Singleton);
            builder.Register<UiInitializer>(Lifetime.Scoped);
        }

        async Awaitable<IResourceBinder> ISessionAsyncResourceLoader.LoadSessionResourcesAsync(IObjectResolver resolver)
        {
            await resolver.Resolve<WeaponTargetMarkerFactory>().LoadPrefab();
            await LoadAllFeatureWindowsAsync(resolver);
            return null;
        }

        void ISceneFeatureInitializer.OnSceneContainerBuilt(IObjectResolver resolver)
        {
            resolver.Resolve<UiInitializer>();
        }

        private async Awaitable LoadAllFeatureWindowsAsync(IObjectResolver resolver)
        {
            var features = resolver.Resolve<FeaturesModulesList>();
            var windowsManager = resolver.Resolve<WindowsManager>();
            var bindersList = new List<IWindowBinder>();
            foreach (var feature in features.Modules)
            {
                if (feature is IAsyncWindowLoader windowLoader)
                    bindersList.Add(windowLoader.GetWindowBinder());
            }

            var count = bindersList.Count;
            if (count != 0)
            {
                if (count == 1)
                {
                    await bindersList[0].LoadWindow();
                    bindersList[0].RegisterWindow(windowsManager);
                }
                else
                {
                    var awaitables = new Awaitable[count];
                    for (var i = 0; i < count; i++)
                    {
                        awaitables[i] = bindersList[i].LoadWindow();
                    }
                    await AwaitablesExtensions.WhenAll(awaitables);

                    foreach (var binder in bindersList)
                        binder.RegisterWindow(windowsManager);
                }
            }
            //UnityEngine.Debug.Log($"windows loaded: {count}");
        }
    }
}
