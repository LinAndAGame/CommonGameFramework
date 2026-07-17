namespace CommonGameFramework.Presentation
{
    /// <summary>
    /// 表现通道标识基类；游戏项目派生子类并持有单例实例，框架按引用路由。
    /// </summary>
    public abstract class PresentationChannel
    {
    }

    /// <summary>
    /// 无并行需求时的默认单通道。
    /// </summary>
    public sealed class DefaultPresentationChannel : PresentationChannel
    {
        public static DefaultPresentationChannel Instance { get; } = new DefaultPresentationChannel();
    }
}
