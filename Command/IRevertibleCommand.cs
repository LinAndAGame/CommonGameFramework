namespace CardMaster.Framework.Command
{
    /// <summary>
    /// 可撤销的 Command；需要 Revert 时额外实现此接口。
    /// </summary>
    public interface IRevertibleCommand : IGameCommand
    {
        void Revert(CommandContext context);
    }
}
