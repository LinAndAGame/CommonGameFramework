namespace CommonGameFramework.Audio
{
    /// <summary>
    /// 全局音频门面：调用方只碰本类；启动时替换 Player（默认 NullAudioPlayer）。
    /// </summary>
    public static class Audio
    {
        static IAudioPlayer _player = NullAudioPlayer.Instance;

        public static IAudioPlayer Player
        {
            get => _player;
            set => _player = value ?? NullAudioPlayer.Instance;
        }

        public static float MasterVolume
        {
            get => _player.MasterVolume;
            set => _player.MasterVolume = value;
        }

        public static float SfxVolume
        {
            get => _player.SfxVolume;
            set => _player.SfxVolume = value;
        }

        public static float BgmVolume
        {
            get => _player.BgmVolume;
            set => _player.BgmVolume = value;
        }

        public static void PlaySfx(AudioId id, float volumeScale = 1f) =>
            _player.PlaySfx(id, volumeScale);

        public static void PlayBgm(AudioId id, bool loop = true, float volumeScale = 1f) =>
            _player.PlayBgm(id, loop, volumeScale);

        public static void StopBgm() => _player.StopBgm();

        public static void PauseBgm() => _player.PauseBgm();

        public static void ResumeBgm() => _player.ResumeBgm();
    }
}
