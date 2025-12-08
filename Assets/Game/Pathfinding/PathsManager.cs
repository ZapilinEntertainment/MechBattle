using Pathfinding;

namespace ZE.MechBattle.Ecs.Pathfinding
{
    public class PathsManager : IntEncodingDictionary<PooledPath>
    {
        public override void Unregister(int key)
        {
            if (Dictionary.TryGetValue(key, out var pooledPath))
            {
                ReleasePath(pooledPath);
                Dictionary.Remove(key);
            }
        }

        public override void Dispose()
        {
            foreach (var pooledPath in Dictionary.Values) 
            {
               ReleasePath(pooledPath);
            }
            base.Dispose();
        }

        public bool TryGetPath(int token, out ABPath path)
        {
            if (!TryGetElement(token, out var pooledPath))
            {
                UnityEngine.Debug.LogError("path token expired!");
                path = default;
                return false;
            }

            if (pooledPath.Path.IsDone())
            {
                if (pooledPath.Path.error)
                {
                    path = default;
                    return false;
                }

                path = pooledPath.Path;
                return true;
            }
            else
            {
                path = default;
                return false;
            }
        }

        public override void OnElementAdded(int key, PooledPath value) => value.Path.Claim(value.Holder);

        private void ReleasePath(in PooledPath pooledPath) => pooledPath.Path.Release(pooledPath.Holder);
    }
}
