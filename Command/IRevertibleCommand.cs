namespace CommonGameFramework.Command
{
    /// <summary>
    /// 可撤销的 Command；需要 Revert 时额外实现此接口。调用方自行 Revert(context)。
    /// </summary>
    public interface IRevertibleCommand : IGameCommand
    {
        void Revert(CommandContext context);
    }
}
