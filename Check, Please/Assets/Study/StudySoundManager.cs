using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class StudySoundManager : MonoBehaviour
{
    public static StudySoundManager Instance;

    public AudioSource BGMAudioSourse;
    public AudioSource SFXAudioSourse;

    Dictionary<string, AudioClip> BGMClips = new Dictionary<string, AudioClip>();
    Dictionary<string, AudioClip> SFXClips = new Dictionary<string, AudioClip>();

    [System.Serializable]
    public struct NamedAudioClip
    {
        public string name;
        public AudioClip clip;
    }

    public NamedAudioClip[] BGMClipList;
    public NamedAudioClip[] SFXClipList;

    void InitializedAudioClips()
    {
        foreach (var bgm in BGMClipList)
        {
            if(!BGMClips.ContainsKey(bgm.name))
            {
                BGMClips.Add(bgm.name,bgm.clip);
            }
        }
        foreach (var sfx in SFXClipList)
        {
            if (!SFXClips.ContainsKey(sfx.name))
            {
                SFXClips.Add(sfx.name, sfx.clip);
            }
        }
    }
    private void Awake()
    {
        if(Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
        InitializedAudioClips();
    }

    public void PlayBGM(string name)
    {
        if(BGMClips.ContainsKey(name))
        {
            BGMAudioSourse.clip = BGMClips[name];
            BGMAudioSourse.Play();
        }
        else
        {
            Debug.LogError($"BGM : {name}가 없습니다.");
        }
    }
    public void PlaySFX(string name)
    {
        if (SFXClips.ContainsKey(name))
        {
            SFXAudioSourse.clip = SFXClips[name];
            SFXAudioSourse.Play();
        }
        else
        {
            Debug.LogError($"SFX : {name}가 없습니다.");
        }
    }
    public void SetBGMVolume(float volume)
    {
        BGMAudioSourse.volume = Mathf.Clamp(volume, 0.0f, 1.0f);
    }
    public void SetSFXVolume(float volume)
    {
        SFXAudioSourse.volume = Mathf.Clamp(volume, 0.0f, 1.0f);
    }
    public void StopBGM()
    {
        BGMAudioSourse.Stop();
    }
    public void StopSFX()
    {
        SFXAudioSourse.Stop();
    }
}
