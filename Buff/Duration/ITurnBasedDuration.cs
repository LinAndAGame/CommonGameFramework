namespace CommonGameFramework.Buff
{
    /// <summary>
    /// 回合制倒计时能力；由 BuffContainer.AdvanceTurn 驱动。
    /// </summary>
    public interface ITurnBasedDuration : IExpirable
    {
        void AdvanceTurn();
    }
}
