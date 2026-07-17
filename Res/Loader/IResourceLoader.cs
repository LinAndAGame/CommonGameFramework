using Cysharp.Threading.Tasks;
using UnityEngine;
using Object = UnityEngine.Object;

namespace CommonGameFramework.Res
{
    /// <summary>
    /// 资源加载 seam；默认 adapter 为 Resources，游戏层可替换为 Addressables 等。
    /// </summary>
    public interface IResourceLoader
    {
        T Load<T>(string path) where T : Object;
        UniTask<T> LoadAsync<T>(string path) where T : Object;
    }
}
