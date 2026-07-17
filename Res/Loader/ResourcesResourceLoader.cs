using Cysharp.Threading.Tasks;
using UnityEngine;
using Object = UnityEngine.Object;

namespace CommonGameFramework.Res
{
    /// <summary>
    /// 基于 Unity Resources 的默认 IResourceLoader adapter。
    /// </summary>
    public sealed class ResourcesResourceLoader : IResourceLoader
    {
        public T Load<T>(string path) where T : Object
        {
            if (string.IsNullOrEmpty(path))
            {
                Debug.LogError("[Res] 资源路径为空。");
                return null;
            }

            var asset = Resources.Load<T>(path);
            if (asset == null)
                Debug.LogWarning($"[Res] 未找到资源：{path}（{typeof(T).Name}）");

            return asset;
        }

        public async UniTask<T> LoadAsync<T>(string path) where T : Object
        {
            if (string.IsNullOrEmpty(path))
            {
                Debug.LogError("[Res] 资源路径为空。");
                return null;
            }

            var request = Resources.LoadAsync<T>(path);
            await request;
            var asset = request.asset as T;
            if (asset == null)
                Debug.LogWarning($"[Res] 未找到资源：{path}（{typeof(T).Name}）");

            return asset;
        }
    }
}
