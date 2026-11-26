using UnityEngine;
using UnityEngine.ResourceManagement.AsyncOperations;
using ZE.MechBattle.AssetsManagement;

namespace ZE.MechBattle
{
    public class GameObjectAssetProvider : AssetProvider<GameObject>, IMonoAssetProvider
    {
        public GameObjectAssetProvider(AsyncOperationHandle<GameObject> handle) : base(handle)
        {
        }

        public ComponentType GetComponent<ComponentType>() where ComponentType : MonoBehaviour => _handle.Result.GetComponent<ComponentType>();
    }
}
