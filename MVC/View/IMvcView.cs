namespace CommonGameFramework.MVC
{
    /// <summary>表现契约。只读取快照；具体视图自行管理按钮监听和资源引用。</summary>
    public interface IMvcView<in TSnapshot>
    {
        void Refresh(TSnapshot snapshot);
    }
}
