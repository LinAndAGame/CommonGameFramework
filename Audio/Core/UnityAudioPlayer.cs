using System;
using UnityEngine;

namespace CommonGameFramework.Audio
{
    /// <summary>
    /// Unity AudioSource 实现；Clip 由 resolve 委托解析（游戏层可用 Res.Load 或 Addressables）。
    /// sfxSource / bgmSource 须在场景或 Prefab 中预先配置并注入，禁止运行时 AddComponent。
    /// </summary>
    public sealed class UnityAudioPlayer : IAudioPlayer
    {
        readonly AudioSource _sfxSource;
        readonly AudioSource _bgmSource;
        readonly Func<AudioId, AudioClip> _resolve;

        float _masterVolume = 1f;
        float _sfxVolume = 1f;
        float _bgmVolume = 1f;

        public UnityAudioPlayer(AudioSource sfxSource, AudioSource bgmSource, Func<AudioId, AudioClip> resolve)
        {
            _sfxSource = sfxSource ?? throw new ArgumentNullException(nameof(sfxSource));
            _bgmSource = bgmSource ?? throw new ArgumentNullException(nameof(bgmSource));
            _resolve = resolve ?? throw new ArgumentNullException(nameof(resolve));
        }

        public float MasterVolume
        {
            get => _masterVolume;
            set
            {
                _masterVolume = Mathf.Clamp01(value);
                ApplyBgmVolume();
            }
        }

        public float SfxVolume
        {
            get => _sfxVolume;
            set => _sfxVolume = Mathf.Clamp01(value);
        }

        public float BgmVolume
        {
            get => _bgmVolume;
            set
            {
                _bgmVolume = Mathf.Clamp01(value);
                ApplyBgmVolume();
            }
        }

        public void PlaySfx(AudioId id, float volumeScale = 1f)
        {
            var clip = ResolveClip(id);
            if (clip == null) return;
            _sfxSource.PlayOneShot(clip, _masterVolume * _sfxVolume * Mathf.Max(0f, volumeScale));
        }

        public void PlayBgm(AudioId id, bool loop = true, float volumeScale = 1f)
        {
            var clip = ResolveClip(id);
            if (clip == null) return;

            _bgmSource.clip = clip;
            _bgmSource.loop = loop;
            ApplyBgmVolume(volumeScale);
            _bgmSource.Play();
        }

        public void StopBgm()
        {
            _bgmSource.Stop();
            _bgmSource.clip = null;
        }

        public void PauseBgm() => _bgmSource.Pause();

        public void ResumeBgm() => _bgmSource.UnPause();

        AudioClip ResolveClip(AudioId id)
        {
            if (id == null)
            {
                Debug.LogError("[Audio] AudioId 为空。");
                return null;
            }

            var clip = _resolve(id);
            if (clip == null)
                Debug.LogWarning($"[Audio] 未解析到 AudioClip：{id.Key}");
            return clip;
        }

        void ApplyBgmVolume(float volumeScale = 1f)
        {
            _bgmSource.volume = _masterVolume * _bgmVolume * Mathf.Max(0f, volumeScale);
        }
    }
}
