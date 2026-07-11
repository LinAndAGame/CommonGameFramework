namespace CardMaster.Framework.View
{
    public enum ViewStackMode
    {
        Single,
        Push,
        Replace
    }

    public class ViewOpenOptions
    {
        public UILayer Layer { get; set; } = UILayer.Main;
        public object Param { get; set; }
        public ViewStackMode StackMode { get; set; } = ViewStackMode.Replace;
    }
}
