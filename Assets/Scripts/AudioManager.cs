using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AudioManager : MonoBehaviour
{
    // 오디오 클립 담을 전투 음악
    public AudioClip battleClip;
    // 오디오 소스 컴포넌트
    private AudioSource audioSource;
    // 맞는 소리 클립 모음
    public AudioClip[] painSounds;
    public AudioClip[] dyingSounds;

    void Start()
    {
        // 오디오 소스 컴포넌트 추가
        audioSource = gameObject.AddComponent<AudioSource>();
        // 오디오 클립 설정
        audioSource.clip = battleClip;
        // 반복 재생 여부
        audioSource.loop = false;
        // 볼륨 설정
        audioSource.volume = 0.5f;
        // 오디오 재생
        audioSource.Play();

        //StartCoroutine(PlayAudioAfterDelay(30f));
    }

    void Update()
    {
        
    }


    //// 게임이 시작하고 30초 후에 오디오를 재생한다.
    //IEnumerator PlayAudioAfterDelay(float delay)
    //{
    //    yield return new WaitForSeconds(delay);
    //    audioSource.Play();
    //}

    // 전투 음악 재생
    public void PlayAudio()
    {
        if (audioSource != null && audioSource.clip != null)
        {
            audioSource.Play();
        }
    }

    //// 전투 음악 종료
    //public void StopAudio()
    //{
    //    if (audioSource != null && audioSource.isPlaying)
    //    {
    //        audioSource.Stop();
    //    }
    //}
}
