namespace CommonGameFramework.Buff
{
    /// <summary>
    /// Buff 定义注册表 seam；统一 buffId → BaseBuff → BuffInstance 创建路径。
    /// </summary>
    public interface IBuffRegistry
    {
        /// <summary>注册定义；definition 或 BuffId 无效时返回 false。</summary>
        bool Register(BaseBuff definition);

        bool TryGetDefinition(string buffId, out BaseBuff definition);

        /// <summary>创建实例；未注册 buffId 时返回 false 且 instance 为 null。</summary>
        bool TryCreate(string buffId, object owner, out BuffInstance instance);
    }
}
