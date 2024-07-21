using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AudioManager : MonoBehaviour
{
    // ����� Ŭ�� ���� ���� ����
    public AudioClip battleClip;
    // ����� �ҽ� ������Ʈ
    private AudioSource audioSource;
    // �´� �Ҹ� Ŭ�� ����
    public AudioClip[] painSounds;
    public AudioClip[] dyingSounds;

    void Start()
    {
        // ����� �ҽ� ������Ʈ �߰�
        audioSource = gameObject.AddComponent<AudioSource>();
        // ����� Ŭ�� ����
        audioSource.clip = battleClip;
        // �ݺ� ��� ����
        audioSource.loop = false;
        // ���� ����
        audioSource.volume = 0.5f;
        // ����� ���
        audioSource.Play();

        //StartCoroutine(PlayAudioAfterDelay(30f));
    }

    void Update()
    {
        
    }


    //// ������ �����ϰ� 30�� �Ŀ� ������� ����Ѵ�.
    //IEnumerator PlayAudioAfterDelay(float delay)
    //{
    //    yield return new WaitForSeconds(delay);
    //    audioSource.Play();
    //}

    // ���� ���� ���
    public void PlayAudio()
    {
        if (audioSource != null && audioSource.clip != null)
        {
            audioSource.Play();
        }
    }

    //// ���� ���� ����
    //public void StopAudio()
    //{
    //    if (audioSource != null && audioSource.isPlaying)
    //    {
    //        audioSource.Stop();
    //    }
    //}
}
