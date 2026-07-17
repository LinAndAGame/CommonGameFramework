using Cysharp.Threading.Tasks;
using UnityEngine;
using Object = UnityEngine.Object;

namespace CommonGameFramework.Res
{
    /// <summary>
    /// 资源加载静态门面：调用方只碰本类；实现经 IResourceLoader（见 Loader/）。
    /// </summary>
    public static class Res
    {
        static IResourceLoader _loader = new ResourcesResourceLoader();

        /// <summary>当前 loader adapter；测试或换后端时替换。</summary>
        public static IResourceLoader Loader
        {
            get => _loader;
            set => _loader = value ?? new ResourcesResourceLoader();
        }

        public static T Load<T>(string path) where T : Object => _loader.Load<T>(path);

        public static UniTask<T> LoadAsync<T>(string path) where T : Object => _loader.LoadAsync<T>(path);
    }
}
