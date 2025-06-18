// SoundManager.cs (새로 만들기)
using UnityEngine;
using System.Collections.Generic;

// 재생할 효과음의 종류를 미리 정의합니다.
public enum SfxType
{
    // 플레이어 관련
    PlayerAttack,
    PlayerHit,
    PlayerDie,
    PlayerDefend,
    // 적 관련
    EnemyAttack,
    EnemyHit,
    EnemyDie,
    // UI 및 기타
    ButtonClick,
    RuneSelect,
    CardDraw,
    AtkBtn
}

// 효과음 종류와 실제 오디오 클립을 연결하는 데이터 구조
[System.Serializable]
public class SoundMapping
{
    public SfxType type;
    public AudioClip clip;
}

public class SoundManager : MonoBehaviour
{
    public static SoundManager Instance { get; private set; }

    [Header("사운드 목록")]
    [Tooltip("효과음(SFX) 종류와 오디오 클립을 여기에 모두 등록해주세요.")]
    [SerializeField] private List<SoundMapping> sfxMappings;

    [Header("오디오 소스")]
    private AudioSource bgmSource; // 배경음악(BGM) 재생기
    private AudioSource sfxSource; // 효과음(SFX) 재생기

    // 빠른 조회를 위한 딕셔너리
    private Dictionary<SfxType, AudioClip> sfxDictionary;

    void Awake()
    {
        // 싱글톤 패턴 및 씬 전환 시 파괴되지 않도록 설정
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
            return;
        }

        // 오디오 소스 컴포넌트 추가 및 설정
        bgmSource = gameObject.AddComponent<AudioSource>();
        bgmSource.loop = true; // 배경음악은 반복 재생
        bgmSource.playOnAwake = false;

        sfxSource = gameObject.AddComponent<AudioSource>();
        sfxSource.loop = false;
        sfxSource.playOnAwake = false;

        // 인스펙터에서 설정한 리스트를 딕셔너리로 변환
        sfxDictionary = new Dictionary<SfxType, AudioClip>();
        foreach (var mapping in sfxMappings)
        {
            sfxDictionary[mapping.type] = mapping.clip;
        }
    }

    /// <summary>
    /// 지정된 효과음을 한 번 재생합니다.
    /// </summary>
    /// <param name="type">재생할 효과음의 종류</param>
    public void PlaySfx(SfxType type)
    {
        if (sfxDictionary.TryGetValue(type, out AudioClip clip))
        {
            // ▼▼▼ 여기에 Null 체크를 추가하면 더 안전합니다. ▼▼▼
            if (clip != null)
            {
                sfxSource.PlayOneShot(clip);
            }
            else
            {
                Debug.LogWarning($"Sfx Mappings 리스트의 '{type}' 타입에 오디오 클립이 비어있습니다(None).");
            }
        }
        else
        {
            Debug.LogWarning($"'{type}'에 해당하는 효과음 타입이 Sfx Mappings 리스트에 등록되지 않았습니다.");
        }
    }

    /// <summary>
    /// 배경음악을 재생합니다.
    /// </summary>
    /// <param name="bgmClip">재생할 배경음악 오디오 클립</param>
    public void PlayBgm(AudioClip bgmClip)
    {
        // 현재 재생 중인 음악과 다른 경우에만 새로 재생
        if (bgmSource.clip == bgmClip && bgmSource.isPlaying) return;

        bgmSource.clip = bgmClip;
        bgmSource.Play();
    }

    /// <summary>
    /// 배경음악 재생을 멈춥니다.
    /// </summary>
    public void StopBgm()
    {
        bgmSource.Stop();
    }
}