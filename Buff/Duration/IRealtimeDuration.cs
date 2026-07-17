namespace CommonGameFramework.Buff
{
    /// <summary>
    /// 实时倒计时能力；由 BuffContainer.TickRealtime 驱动。
    /// </summary>
    public interface IRealtimeDuration : IExpirable
    {
        void Tick(float deltaTime);
    }
}
