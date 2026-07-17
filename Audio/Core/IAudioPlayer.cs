namespace CommonGameFramework.Audio
{
    /// <summary>
    /// 音频播放 seam；Clip 解析由构造时注入，框架不绑定具体资源路径约定。
    /// </summary>
    public interface IAudioPlayer
    {
        float MasterVolume { get; set; }
        float SfxVolume { get; set; }
        float BgmVolume { get; set; }

        void PlaySfx(AudioId id, float volumeScale = 1f);
        void PlayBgm(AudioId id, bool loop = true, float volumeScale = 1f);
        void StopBgm();
        void PauseBgm();
        void ResumeBgm();
    }
}
