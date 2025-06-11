using UnityEngine;

public class BtSoundManager: MonoBehaviour
{
    public AudioClip clickSound; // 버튼 클릭 사운드
    private AudioSource audioSource;

    private void Awake()
    {
        // AudioSource 컴포넌트 가져오기 또는 추가하기
        audioSource = GetComponent<AudioSource>();
        if (audioSource == null)
        {
            audioSource = gameObject.AddComponent<AudioSource>();
        }

        // AudioSource 초기 설정
        audioSource.playOnAwake = false;
    }

    public void PlayClickSound()
    {
        if (audioSource != null && clickSound != null)
        {
            audioSource.PlayOneShot(clickSound); // 단발성 사운드 재생
        }
    }
}
