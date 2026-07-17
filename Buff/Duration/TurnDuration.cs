namespace CommonGameFramework.Buff
{
    /// <summary>
    /// 回合制持续时间。
    /// </summary>
    public class TurnDuration : ITurnBasedDuration
    {
        int _remainingTurns;

        public TurnDuration(int turns)
        {
            _remainingTurns = turns;
        }

        public bool IsExpired => _remainingTurns <= 0;

        public void AdvanceTurn()
        {
            _remainingTurns--;
        }
    }
}
