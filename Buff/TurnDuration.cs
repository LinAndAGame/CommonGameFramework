namespace CardMaster.Framework.Buff
{
    /// <summary>
    /// 回合制持续时间（占位实现，后续接入战斗回合系统）。
    /// </summary>
    public class TurnDuration : IBuffDuration
    {
        private int _remainingTurns;

        public TurnDuration(int turns)
        {
            _remainingTurns = turns;
        }

        public bool IsExpired => _remainingTurns <= 0;

        public void Tick(float deltaTime)
        {
            // 占位：回合推进由外部调用 AdvanceTurn 处理
        }

        public void AdvanceTurn()
        {
            _remainingTurns--;
        }
    }
}
