namespace CommonGameFramework.View
{
    /// <summary>入栈策略：Push 压栈并隐藏下层；Replace 清空该层再打开。</summary>
    public enum ViewStackMode
    {
        Push,
        Replace
    }

    /// <summary>OpenAsync 参数：所属层、栈模式、可选传参。</summary>
    public class ViewOpenOptions
    {
        public ViewLayer Layer { get; set; } = DefaultViewLayer.Instance;
        public ViewStackMode StackMode { get; set; } = ViewStackMode.Push;
        public object Param { get; set; }
    }
}
