namespace CardMaster.Framework.Command
{
    public interface IGameCommand
    {
        void Execute(CommandContext context);
    }
}
