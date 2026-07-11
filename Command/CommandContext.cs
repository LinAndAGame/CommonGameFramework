using CardMaster.Framework.Buff;
using CardMaster.Framework.Command;
using CardMaster.Framework.Stat;

namespace CardMaster.Framework.Command
{
    public class CommandContext
    {
        public BuffContainer BuffContainer { get; set; }
        public CommandExecutor Executor { get; set; }
        public object Source { get; set; }
        public object Target { get; set; }
        public GameStat TargetHp { get; set; }
    }
}
