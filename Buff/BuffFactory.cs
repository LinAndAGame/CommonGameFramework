namespace CardMaster.Framework.Buff
{
    /// <summary>
    /// Buff 工厂占位，后续接入配置表创建实例。
    /// </summary>
    public static class BuffFactory
    {
        public static BuffInstance Create(string buffId, object owner)
        {
            // 占位：由具体 Buff 子类或导表数据扩展
            return null;
        }
    }
}
