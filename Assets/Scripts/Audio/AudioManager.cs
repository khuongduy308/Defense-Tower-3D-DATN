using System;
using System.Collections.Generic;
using UnityEngine;

public class AudioManager : MonoBehaviour
{
    public static AudioManager Instance { get; private set; }

    [Header("Audio Collections")]
    public Sound[] musicTracks; // Danh sách nhạc nền
    public Sound[] sfxSounds;   // Danh sách hiệu ứng (bắn, nổ, click)

    [Header("Sources")]
    [SerializeField] private AudioSource musicSource;
    [SerializeField] private AudioSource sfxSource;

    private void Awake()
    {
        // Setup Singleton
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
        DontDestroyOnLoad(gameObject); // Giữ cho Audio không bị tắt khi chuyển Scene
    }

    private void Start()
    {
        // Cài đặt âm lượng ban đầu
        // musicSource.volume = musicVolume;
        // sfxSource.volume = sfxVolume;
    }

    // --- PHẦN NHẠC NỀN (MUSIC) ---
    public void PlayMusic(AudioClip clip)
    {
        // Nếu chưa gán loa thì thôi
        if (musicSource == null || clip == null) return;

        // Nếu bài đang hát trùng với bài mới thì thôi không hát lại (đỡ bị ngắt quãng)
        if (musicSource.clip == clip && musicSource.isPlaying) return;

        musicSource.clip = clip;
        musicSource.loop = true; // Nhạc nền luôn lặp
        musicSource.Play();
    }

    // public void PlayRandomMusic()
    // {
    //     if(musicTracks.Length == 0) return;

    //     int randomIndex = UnityEngine.Random.Range(0, musicTracks.Length);
    //     Sound randomSound = musicTracks[randomIndex];

    //     PlayMusicObject(randomSound);
    // }

    public void StopMusic()
    {
        musicSource.Stop();
    }

    // --- PHẦN HIỆU ỨNG (SFX) ---
    public void PlaySFX(string name)
    {
        Sound s = Array.Find(sfxSounds, sound => sound.name == name);
        if (s == null)
        {
            Debug.LogWarning("Không tìm thấy SFX: " + name);
            return;
        }

        // PlayOneShot cho phép nhiều âm thanh chồng lên nhau (VD: nhiều tháp bắn cùng lúc)
        sfxSource.PlayOneShot(s.clip, s.volume);
    }

    public void PlayRandomSFX(params string[] soundNames)
    {
        // 1. Kiểm tra danh sách có rỗng không
        if (soundNames == null || soundNames.Length == 0)
        {
            Debug.LogWarning("Danh sách SFX rỗng!");
            return;
        }

        int randomIndex = UnityEngine.Random.Range(0, soundNames.Length);

        string selectedName = soundNames[randomIndex];
        PlaySFX(selectedName);
    }

    // --- CÀI ĐẶT CHUNG (VOLUME/MUTE) ---
    public void SetMusicVolume(float volume)
    {
        musicSource.volume = volume;
    }

    public void SetSFXVolume(float volume)
    {
        sfxSource.volume = volume;
    }
    
    public void ToggleMusic(bool isOn)
    {
        musicSource.mute = !isOn;
    }

    public void ToggleSFX(bool isOn)
    {
        sfxSource.mute = !isOn;
    }
}

