namespace CardMaster.Framework.Buff
{
    /// <summary>
    /// Buff 持续时间策略接口。
    /// </summary>
    public interface IBuffDuration
    {
        bool IsExpired { get; }
        void Tick(float deltaTime);
    }
}
