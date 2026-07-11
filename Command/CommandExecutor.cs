namespace CardMaster.Framework.Command
{
    public class CommandExecutor
    {
        public void Execute(IGameCommand command, CommandContext context)
        {
            if (command == null || context == null) return;
            command.Execute(context);
        }

        public void Revert(IRevertibleCommand command, CommandContext context)
        {
            if (command == null || context == null) return;
            command.Revert(context);
        }
    }
}
