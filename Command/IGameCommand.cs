namespace CommonGameFramework.Command
{
    /// <summary>
    /// 同步逻辑 Command：立刻修改权威游戏状态，不含 await。
    /// 嵌套子 Command 直接 child.Execute(context)；表现动效用 Presentation，勿写在此处。
    /// </summary>
    public interface IGameCommand
    {
        void Execute(CommandContext context);
    }
}
