using Cysharp.Threading.Tasks;
using UnityEngine;
using Object = UnityEngine.Object;

namespace CardMaster.Framework.Res
{
    /// <summary>
    /// Resources 加载门面，提供同步与异步加载。
    /// </summary>
    public static class Res
    {
        /// <summary>同步加载 Resources 资源。</summary>
        public static T Load<T>(string path) where T : Object
        {
            if (string.IsNullOrEmpty(path))
            {
                Debug.LogError("[Res] 资源路径为空。");
                return null;
            }

            var asset = Resources.Load<T>(path);
            if (asset == null)
            {
                Debug.LogWarning($"[Res] 未找到资源：{path}（{typeof(T).Name}）");
            }

            return asset;
        }

        /// <summary>异步加载 Resources 资源。</summary>
        public static async UniTask<T> LoadAsync<T>(string path) where T : Object
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
            {
                Debug.LogWarning($"[Res] 未找到资源：{path}（{typeof(T).Name}）");
            }

            return asset;
        }
    }
}
