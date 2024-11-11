using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlaySoundManager : MonoBehaviour
{
    public static PlaySoundManager Instance;
    AudioSource bgSource;
    AudioSource effectSource;

    public string SoundDir = "Sounds";
    private void Awake()
    {
        Instance = this;
        bgSource = gameObject.AddComponent<AudioSource>();
        bgSource.loop = true;

        effectSource = gameObject.AddComponent<AudioSource>();
        effectSource.loop = false;
    }
    void Start()
    {
        
    }

    public void PlayBg(string audioName,float volume=1) 
    {
        if (bgSource.clip == null|| bgSource.clip.name!= audioName) 
        {
            AudioClip clip= Resources.Load<AudioClip>(SoundDir+"/"+audioName);
            if (clip != null)
            {
                bgSource.clip = clip;
                bgSource.Play();
                bgSource.loop = true;
                bgSource.volume = volume;
            }
        }
    }
    public void StopBg() 
    {
        if (bgSource.isPlaying) 
        {
            bgSource.Stop();
            bgSource.clip = null;
        }
    }

    public void PlayEffect(string audioName, float volume = 1)
    {
        AudioClip clip = Resources.Load<AudioClip>(SoundDir + "/" + audioName);
        if (clip != null)
        {
            effectSource.PlayOneShot(clip,volume);
        }
    } 

}
