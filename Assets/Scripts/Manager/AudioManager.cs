using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AudioManager : MonoBehaviour
{
    public AudioClip bgmClip; // Inspector에서 BGM 파일을 연결할 변수
    private AudioSource audioSource;

    void Awake()
    {
        audioSource = GetComponent<AudioSource>();
        if (audioSource == null)
        {
            audioSource = gameObject.AddComponent<AudioSource>();
        }

        // AudioSource 설정
        audioSource.clip = bgmClip;
        audioSource.loop = true; // BGM 반복 재생
        audioSource.playOnAwake = true;

    }

    // Start is called before the first frame update
    void Start()
    {
        PlayBGM();
    }

    public void PlayBGM()
    {
        if (audioSource != null && bgmClip != null)
        {
            audioSource.Play();
        }
    }

    // Update is called once per frame
    void Update()
    {

    }
}
