using UnityEngine;

public class AudioManager : MonoBehaviour
{
    // 이 씬에서 재생할 BGM 파일을 Inspector에서 연결합니다.
    public AudioClip bgmClip;

    void Start()
    {
        // 중앙 SoundManager가 있는지, BGM 클립이 할당되었는지 확인합니다.
        if (SoundManager.Instance != null && bgmClip != null)
        {
            // AudioManager가 직접 재생하는 대신, SoundManager에게 BGM 재생을 '요청'합니다.
            SoundManager.Instance.PlayBgm(bgmClip);
        }
        else
        {
            // 문제가 있을 경우 원인을 파악하기 쉽도록 로그를 남깁니다.
            if (SoundManager.Instance == null)
            {
                Debug.LogError("SoundManager 인스턴스를 찾을 수 없습니다! 씬에 SoundManager가 있는지 확인해주세요.");
            }
            if (bgmClip == null)
            {
                Debug.LogWarning("AudioManager에 bgmClip이 할당되지 않았습니다.", this.gameObject);
            }
        }
    }
}