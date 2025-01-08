using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AudioManager : MonoBehaviour
{
    public AudioClip battleClip;
    private AudioSource audioSource;
    public AudioClip[] painSounds;
    public AudioClip[] dyingSounds;

    void Start()
    {
        audioSource = gameObject.AddComponent<AudioSource>();
        audioSource.clip = battleClip;
        audioSource.loop = false;
        audioSource.volume = 0.5f;
        // 재생
        audioSource.Play();
    }

    public void PlayAudio()
    {
        if (audioSource != null && audioSource.clip != null)
        {
            audioSource.Play();
        }
    }
}
