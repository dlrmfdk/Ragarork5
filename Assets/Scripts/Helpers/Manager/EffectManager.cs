// EffectManager.cs (새로 만들기)
using UnityEngine;
using System.Collections.Generic;

// 재생할 이펙트의 종류를 미리 정의합니다.
public enum EffectType
{
    RuneClick,         // 덱에서 룬을 클릭했을 때
    RedRuneImpact,     // 빨간 룬으로 적을 타격했을 때
    BlueRuneBuff,      // 파란 룬을 자신에게 사용했을 때
    PlayerHit,         // 플레이어가 피격당했을 때
    EnemyHit,          // 적이 피격당했을 때
    EnemyDeath,         // 적이 죽었을 때
    PlayerAttackCast // 플레이어가 공격 시
}

// 이펙트 종류와 실제 프리팹을 연결해주는 데이터 구조
[System.Serializable]
public class EffectMapping
{
    public EffectType type;
    public GameObject effectPrefab;
}

public class EffectManager : MonoBehaviour
{
    public static EffectManager Instance { get; private set; }

    [Header("이펙트 목록")]
    [Tooltip("이펙트 종류와 프리팹을 여기에 모두 등록해주세요.")]
    [SerializeField] private List<EffectMapping> effectMappings;

    // 빠른 조회를 위한 딕셔너리
    private Dictionary<EffectType, GameObject> effectDictionary;

    void Awake()
    {
        // 싱글톤 패턴
        if (Instance == null)
        {
            Instance = this;
            // DontDestroyOnLoad(gameObject); // 게임 전체에 걸쳐 필요하다면 주석 해제
        }
        else
        {
            Destroy(gameObject);
            return;
        }

        // 인스펙터에서 설정한 리스트를 딕셔너리로 변환하여 사용하기 쉽게 만듭니다.
        effectDictionary = new Dictionary<EffectType, GameObject>();
        foreach (var mapping in effectMappings)
        {
            effectDictionary[mapping.type] = mapping.effectPrefab;
        }
    }

    /// <summary>
    /// 지정된 위치에 이펙트를 재생합니다. (한 번 재생되고 사라지는 이펙트용)
    /// </summary>
    /// <param name="type">재생할 이펙트의 종류</param>
    /// <param name="position">이펙트가 나타날 월드 좌표</param>
    public void PlayEffect(EffectType type, Vector3 position)
    {
        if (effectDictionary.TryGetValue(type, out GameObject effectPrefab))
        {
            Instantiate(effectPrefab, position, Quaternion.identity);
        }
        else
        {
            Debug.LogWarning($"'{type}'에 해당하는 이펙트 프리팹이 등록되지 않았습니다.");
        }
    }

    /// <summary>
    /// 지정된 부모 오브젝트에 이펙트를 붙여서 재생합니다. (버프/오라처럼 대상을 따라다녀야 하는 이펙트용)
    /// </summary>
    /// <param name="type">재생할 이펙트의 종류</param>
    /// <param name="parent">이펙트가 자식으로 붙을 부모 Transform</param>
    public void PlayEffect(EffectType type, Transform parent)
    {
        if (effectDictionary.TryGetValue(type, out GameObject effectPrefab))
        {
            Instantiate(effectPrefab, parent);
        }
        else
        {
            Debug.LogWarning($"'{type}'에 해당하는 이펙트 프리팹이 등록되지 않았습니다.");
        }
    }
}