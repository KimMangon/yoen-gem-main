using UnityEngine;
using System.Collections.Generic;

public class AudioManager : MonoBehaviour
{
    public static AudioManager Instance;

    public enum SFX
    {
        Hit, Swing, Shoot, Item, Augment, Button, Chest, Clear, Defeat
    }

    public enum BGM
    {
        Menu, InGame
    }

    [System.Serializable]
    public class SFXClip
    {
        public SFX type;
        public AudioClip clip;
    }

    [System.Serializable]
    public class BGMClip
    {
        public BGM type;
        public AudioClip clip;
    }

    [Header("Audio Sources")]
    public AudioSource bgmSource;
    public AudioSource sfxSource;

    [Header("SFX Clips")]
    public SFXClip[] sfxClips;
    private Dictionary<SFX, AudioClip> sfxDict;

    [Header("BGM Clips")]
    public BGMClip[] bgmClips;
    private Dictionary<BGM, AudioClip> bgmDict;

    [Header("Volume")]
    public float masterVolume = 1f;
    public float bgmVolume = 1f;
    public float sfxVolume = 1f;

    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
            return;
        }

        sfxDict = new Dictionary<SFX, AudioClip>();
        foreach (var s in sfxClips)
            sfxDict[s.type] = s.clip;

        bgmDict = new Dictionary<BGM, AudioClip>();
        foreach (var b in bgmClips)
            bgmDict[b.type] = b.clip;

        LoadVolumeSettings();
    }

    public void Play(SFX type)
    {
        if (sfxDict.TryGetValue(type, out AudioClip clip) && clip != null)
            sfxSource.PlayOneShot(clip, sfxVolume * masterVolume);
    }

    public void PlayBGM(BGM type)
    {
        if (bgmDict.TryGetValue(type, out AudioClip clip) && clip != null)
        {
            if (bgmSource.clip == clip) return;
            bgmSource.clip = clip;
            bgmSource.Play();
        }
    }

    public void SetMasterVolume(float value)
    {
        masterVolume = value;
        ApplyVolume();
        SaveVolumeSettings();
    }

    public void SetBGMVolume(float value)
    {
        Debug.Log("SetBGMVolume 받은 값: " + value);
        bgmVolume = value;
        ApplyVolume();
        SaveVolumeSettings();
    }

    public void SetSFXVolume(float value)
    {
        sfxVolume = value;
        SaveVolumeSettings();
    }

    void ApplyVolume()
    {
        bgmSource.volume = bgmVolume * masterVolume;
        Debug.Log("BGM 볼륨 적용: " + bgmSource.volume);
    }

    void SaveVolumeSettings()
    {
        PlayerPrefs.SetFloat("MasterVolume", masterVolume);
        PlayerPrefs.SetFloat("BGMVolume", bgmVolume);
        PlayerPrefs.SetFloat("SFXVolume", sfxVolume);
        PlayerPrefs.Save();
    }

    void LoadVolumeSettings()
    {
        masterVolume = PlayerPrefs.GetFloat("MasterVolume", 1f);
        bgmVolume = PlayerPrefs.GetFloat("BGMVolume", 1f);
        sfxVolume = PlayerPrefs.GetFloat("SFXVolume", 1f);
        ApplyVolume();
    }
}