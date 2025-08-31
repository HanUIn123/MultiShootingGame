using UnityEngine;
using System.Collections.Generic;

public class SoundManager : MonoBehaviour
{
    public static SoundManager Instance;

    public AudioSource bgmSource;
    public AudioSource sfxSource;

    // BGM�� ���� ���̺�
    private Dictionary<string, float> bgmVolumes = new Dictionary<string, float>()
    {
        { "BGM", 0.4f },
        { "Stage1BGM", 0.3f },
        { "BossBGM", 1.0f }
    };

    // SFX�� ���� ���̺�
    private Dictionary<string, float> sfxVolumes = new Dictionary<string, float>()
    {
        { "FireSound", 1.0f },
        { "BulletSound", 0.2f }
        //{ "Explosion", 0.8f }
    };

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }

    // �̸����� BGM ��� + ���� �ڵ� ����
    public void PlayBGM(string clipName)
    {
        AudioClip clip = LoadClip(clipName);
        if (clip != null)
        {
            float vol = bgmVolumes.ContainsKey(clipName) ? bgmVolumes[clipName] : 1.0f;
            bgmSource.volume = vol;
            bgmSource.clip = clip;
            bgmSource.Play();
        }
    }

    public void StopBGM()
    {
        bgmSource.Stop();
    }

    // �̸����� SFX ���
    public void PlaySFX(string clipName)
    {
        AudioClip clip = LoadClip(clipName);
        if (clip != null)
        {
            float vol = sfxVolumes.ContainsKey(clipName) ? sfxVolumes[clipName] : 1.0f;
            sfxSource.PlayOneShot(clip, vol);
        }
    }

    // AudioClip ���� �־ SFX ���
    public void PlaySFX(AudioClip clip)
    {
        if (clip != null)
            sfxSource.PlayOneShot(clip);
    }

    // Resources/Sounds/ �������� Ŭ�� �ε�
    public AudioClip LoadClip(string path)
    {
        return Resources.Load<AudioClip>("Sounds/" + path);
    }

    public void SetBGMVolume(float volume)
    {
        bgmSource.volume = Mathf.Clamp01(volume);
    }

    public void SetSFXVolume(float volume)
    {
        sfxSource.volume = Mathf.Clamp01(volume);
    }
}
