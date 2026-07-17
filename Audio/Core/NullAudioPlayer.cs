namespace CommonGameFramework.Audio
{
    /// <summary>
    /// 空实现：默认占位与单测用；启动时请替换为 UnityAudioPlayer。
    /// </summary>
    public sealed class NullAudioPlayer : IAudioPlayer
    {
        public static NullAudioPlayer Instance { get; } = new NullAudioPlayer();

        public float MasterVolume { get; set; } = 1f;
        public float SfxVolume { get; set; } = 1f;
        public float BgmVolume { get; set; } = 1f;

        public void PlaySfx(AudioId id, float volumeScale = 1f) { }
        public void PlayBgm(AudioId id, bool loop = true, float volumeScale = 1f) { }
        public void StopBgm() { }
        public void PauseBgm() { }
        public void ResumeBgm() { }
    }
}
