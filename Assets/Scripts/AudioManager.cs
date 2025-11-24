
using System.Collections.Generic;
using UnityEngine;

public class AudioManager : MonoBehaviour
{
    public static AudioManager Instance;
    public AudioSource audioSource;
    public List<AudioClip> audioClips;
    private void Awake()
    {
        Instance = this;
    }

    public void PlaySound(string clipName)
    {
        AudioClip clip = FindAudioClip(clipName);
        if (clip == null)
        {
            Debug.LogWarning("Clip not found: " + clipName);
            return;
        }
        audioSource.PlayOneShot(clip);
    }

    public AudioClip FindAudioClip(string nameSound)
    {
         return audioClips.Find(x => x.name == nameSound); 
    }
    
}
