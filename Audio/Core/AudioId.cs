namespace CommonGameFramework.Audio
{
    /// <summary>
    /// 音效/音乐标识基类；游戏项目派生子类，Key 由 IAudioPlayer 的解析器映射到 AudioClip。
    /// </summary>
    public abstract class AudioId
    {
        public abstract string Key { get; }

        public override bool Equals(object obj)
        {
            return obj is AudioId other && Key == other.Key;
        }

        public override int GetHashCode() => Key?.GetHashCode() ?? 0;

        public override string ToString() => Key;
    }
}
