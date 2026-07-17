namespace CommonGameFramework.View
{
    /// <summary>
    /// 视图层标识基类；游戏项目派生子类，UICanvasRoot 按实例注册 Canvas。
    /// </summary>
    public abstract class ViewLayer
    {
    }

    /// <summary>
    /// 无多层需求时的默认视图层。
    /// </summary>
    public sealed class DefaultViewLayer : ViewLayer
    {
        public static DefaultViewLayer Instance { get; } = new DefaultViewLayer();
    }
}
