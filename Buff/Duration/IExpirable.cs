namespace CommonGameFramework.Buff
{
    /// <summary>
    /// Buff 过期检测能力。
    /// </summary>
    public interface IExpirable
    {
        bool IsExpired { get; }
    }
}
