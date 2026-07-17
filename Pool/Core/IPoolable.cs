namespace CommonGameFramework.Pool
{
    /// <summary>
    /// 可池化对象接口，在取出/回收时回调。
    /// </summary>
    public interface IPoolable
    {
        void OnSpawnFromPool();
        void OnReturnToPool();
    }
}
