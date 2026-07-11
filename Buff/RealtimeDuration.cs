namespace CardMaster.Framework.Buff
{
    /// <summary>
    /// 实时倒计时持续时间。
    /// </summary>
    public class RealtimeDuration : IBuffDuration
    {
        private float _remaining;

        public RealtimeDuration(float seconds)
        {
            _remaining = seconds;
        }

        public bool IsExpired => _remaining <= 0f;

        public void Tick(float deltaTime)
        {
            _remaining -= deltaTime;
        }
    }
}
