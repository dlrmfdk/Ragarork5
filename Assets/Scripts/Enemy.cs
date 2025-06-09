using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using Spine.Unity; // Spine 애니메이션 사용을 위해 추가
public class Enemy : MonoBehaviour
{
    [SerializeField] private EnemySO enemyData;
    public EnemySO EnemyData => enemyData; // 공개 읽기 전용 속성

    public CharacterIndent CharacterIndent { get; private set; }

    private int currentHp; //현재 체력
   
    public int currentHealth => currentHp; //외부에서 현재 체력을 읽을 수 있도록 public 속성 추가

    private HpBarController hpBarController; // HPBarController 참조 변수 추가

    // 독 효과를 위한 변수 (현재 독 스택)
    private int poisonStack = 0;

    //화상 효과 관련 변수 추가 
    private int burnDamagePerTurn = 0;
    private int burnTurnsRemaining = 0;

    //출혈 효과 관련 변수 추가
    private int bleedDamagePerTurn = 0;
    private int bleedTurnsRemaining = 0;


    private SkeletonAnimation skeletonAnimation; // Spine 애니메이션 컴포넌트 참조



    void Awake()
    {
        CharacterIndent = new CharacterIndent();

        // SkeletonAnimation 컴포넌트 가져오기
        skeletonAnimation = GetComponent<SkeletonAnimation>();
        if (skeletonAnimation == null)
        {
            skeletonAnimation = GetComponentInChildren<SkeletonAnimation>();
            // SkeletonAnimation 컴포넌트가 없는 것은 에러가 아닐 수 있으므로, LogError 대신 Debug.Log 또는 주석 처리합니다.
            // 만약 모든 적이 Spine을 가져야 한다면 LogError가 맞지만, 그렇지 않다면 아래 로그는 필요 없거나 LogWarning 수준입니다.
            if (skeletonAnimation == null)
            {
                // Debug.Log($"SkeletonAnimation component not found on {gameObject.name} or its children. Assuming this enemy does not use Spine animations.");
            }
        }
    }

    public void Initialize(EnemySO data)
    {
        enemyData = data;
        currentHp = enemyData.HP;
        gameObject.name = enemyData.EnemyName;  // 적의 이름 설정

        // 초기 애니메이션 상태를 "Idle"로 설정
        if (skeletonAnimation != null)
        {
            skeletonAnimation.AnimationState.SetAnimation(0, "idle", true); // 트랙 0번, Idle 애니메이션, 반복 true
        }
    }

    public void SetHPBarController(HpBarController controller)
    {
        hpBarController = controller;
        if (hpBarController != null)
        {
            hpBarController.SetTarget(transform);
            hpBarController.SetMaxHP(enemyData.HP);
            hpBarController.SetCurrentHP(currentHp);
        }
    }

    public void PrintEnemyData()
    {
        Debug.Log("적 이름:" + enemyData.EnemyName);
        Debug.Log("적 체력:" + enemyData.HP);
        Debug.Log("적 공격력:" + enemyData.Damage);
        Debug.Log("적 방어력:" + enemyData.Defense);
    }

    // 일반 공격으로 피해를 받는 메소드 (방어력 고려)
    public int Hit(int damage, Player player)
    {
        // 방어력을 고려한 실제 피해량 계산
        int effectiveDamage = Mathf.Max(damage - enemyData.Defense, 0);
        currentHp -= effectiveDamage;
        Debug.Log($"{enemyData.EnemyName}에게 {effectiveDamage}의 피해를 입혔습니다. 남은 체력: {currentHp}");

        // 현재 행동의 총 피해량을 기록하기 위해 BattleContext에 보고
        BattleContext.AddDamage(effectiveDamage);

        // HPBarController가 할당되어 있다면 현재 체력 업데이트
        if (hpBarController != null)
            hpBarController.SetCurrentHP(currentHp);
        else
            Debug.LogWarning("HPBarController가 할당되지 않았습니다.");

        if (currentHp <= 0)
            Die();

        //실제 입힌 피해량 반환
        return effectiveDamage;
    }


    //방어력을 무시하는 직접적인 피해를 받는 메소드 추가
    public void TakeDirectDamage(int damageAmount)
    {
        currentHp -= damageAmount;
        Debug.Log($"{enemyData.EnemyName}이(가) {damageAmount}의 직접 피해를 입었습니다. 남은 체력: {currentHp}");

        if (hpBarController != null)
            hpBarController.SetCurrentHP(currentHp);

        if (currentHp <= 0)
            Die();
    }

    void Die()
    {
        Debug.Log($"{enemyData.EnemyName}이(가) 사망했습니다.");

        if (hpBarController != null)
            Destroy(hpBarController.gameObject);

        // EnemySpawner의 SpawnedEnemies 리스트에서 이 적 제거
        EnemySpawner.Instance.SpawnedEnemies.Remove(this);

        // ─── 여기에 보상 패널 띄우기 추가 ───
        if (EnemySpawner.Instance.SpawnedEnemies.Count == 0)
        {
            // 마지막 적이 죽었을 때
            RewardManager.Instance.ShowRewardPanel();
        }

        Destroy(gameObject);
    }

    public IEnumerator PerformTurn()
    {
        Debug.Log($"{enemyData.EnemyName}의 턴 시작");

        // 턴 시작 시 상태 이상 처리
        ProcessPoisonAtTurnStart(); // 기존 독 처리
        if (currentHp <= 0) yield break; // 독 피해로 사망 시 턴 종료

        ProcessBurnAtTurnStart();   // 화상 피해 처리 추가
        if (currentHp <= 0) yield break; // 화상 피해로 사망 시 턴 종료

        ProcessBleedAtTurnStart();
        if (currentHp <= 0) yield break; // 출혈 피해로 사망 시 턴 종료


        // 독 피해로 인해 적의 현재체력이 0 이하라면 턴을 종료
        if (currentHp <= 0)
        {
            yield break;
        }
        //플레이어에게 적 스탯의 데미지
        yield return StartCoroutine(AttackPlayer(enemyData.Damage));

        // 턴 종료 시 상태 이상 지속시간 감소 등 처리
        ProcessPoisonAtTurnEnd(); //독 지속시간 차감


        Debug.Log($"{enemyData.EnemyName}의 턴 종료");
        yield return null; // 턴 매니저에게 제어권 넘김
    }

    private IEnumerator AttackPlayer(int damage)
    {
        float attackAnimationDuration = 0.5f; // 기본 대기 시간 (애니메이션 못 찾을 경우 대비)

        if (skeletonAnimation != null)
        {
            // "Attack" 애니메이션 재생 (반복 안함)
            Spine.TrackEntry attackTrackEntry = skeletonAnimation.AnimationState.SetAnimation(0, "attack", false);
            if (attackTrackEntry != null && attackTrackEntry.Animation != null)
            {
                attackAnimationDuration = attackTrackEntry.Animation.Duration;
            }
            else
            {
                Debug.LogWarning($"'{gameObject.name}'의 SkeletonAnimation에서 'Attack' 애니메이션을 찾을 수 없습니다. 기본 대기 시간을 사용합니다.");
            }
            // 애니메이션이 끝날 때까지 또는 특정 타격 지점까지 대기
            yield return new WaitForSeconds(attackAnimationDuration); // 전체 애니메이션 길이만큼 대기
        }
        else
        {
            // SkeletonAnimation 컴포넌트가 없으면 기존 방식대로 0.5초 대기
            yield return new WaitForSeconds(0.5f);
        }
        // 실제 공격 (데미지 처리)
        if (Player.Instance != null)
        {
            Player.Instance.TakeDamage(damage); // Player.cs 에 정의된 TakeDamage 호출
            Debug.Log($"{enemyData.EnemyName}이 플레이어에게 {damage}의 데미지를 입혔습니다.");
        }
        else
        {
            Debug.LogError("Player.Instance가 null입니다. 플레이어 공격 불가.");
        }
        // 공격 후 "Idle" 애니메이션으로 전환 (반복)
        if (skeletonAnimation != null)
        {
            skeletonAnimation.AnimationState.SetAnimation(0, "idle", true);
        }
    }

    // ---------------- 독 효과 관련 메서드들을 Enemy 클래스 내부로 이동 ----------------

    /// <summary>
    /// 독 효과를 부여하는 메서드: 독 수치를 증가시킵니다.
    /// </summary>
    public void ApplyPoison(int amount)
    {  
        poisonStack += amount;
        Debug.Log($"{enemyData.EnemyName}에게 {amount}의 독이 부여되었습니다. 총 독 수치: {poisonStack}");
    }

    /// <summary>
    /// 턴 시작 시 독 효과 처리: 현재 독 수치만큼 피해를 입힙니다.
    /// </summary>
    public void ProcessPoisonAtTurnStart()
    {
        if (poisonStack > 0)
        {
            Hit(poisonStack, null);  // 독 피해는 플레이어 인자가 필요 없으므로 null 전달
            Debug.Log($"{enemyData.EnemyName}이(가) 독 피해로 {poisonStack}의 피해를 입었습니다.");
        }
    }

    /// <summary>
    /// 턴 종료 시 독 효과 처리: 독 수치를 1 감소시킵니다.
    /// </summary>
    public void ProcessPoisonAtTurnEnd()
    {
        if (poisonStack > 0)
        {
            poisonStack--;
            Debug.Log($"{enemyData.EnemyName}의 독 수치가 1 감소하여 {poisonStack}이 되었습니다.");
        }
    }
    // --- 화상 효과 관련 메소드 추가 ---
    /// <summary>
    /// 적에게 화상 효과를 적용합니다.
    /// </summary>
    /// <param name="damage">턴당 화상 데미지</param>
    /// <param name="duration">화상 지속 턴 수</param>
    public void ApplyBurn(int damage, int duration)
    {
        burnDamagePerTurn += damage; // 값 갱신 (중첩이 필요하면 로직 변경: +=)
        burnTurnsRemaining = duration;  // 값 갱신 (중첩이 필요하면 로직 변경: += 또는 Max)
        Debug.Log($"{enemyData.EnemyName}에게 {duration}턴 동안 매 턴 {damage}의 화상 효과가 부여되었습니다.");
        // 필요하다면 화상 아이콘 등 UI 표시 로직 추가
    }

    /// <summary>
    /// 턴 시작 시 화상 피해를 처리합니다.
    /// </summary>
    private void ProcessBurnAtTurnStart()
    {
        if (burnTurnsRemaining > 0)
        {
            Debug.Log($"{enemyData.EnemyName}이(가) 화상으로 {burnDamagePerTurn}의 직접피해를 입습니다.");
           
            TakeDirectDamage(burnDamagePerTurn);  //방어도 무시하고 직접 피해 입힘
            burnTurnsRemaining--;

            if (burnTurnsRemaining <= 0)
            {
                burnDamagePerTurn = 0; // 화상 효과 종료
                Debug.Log($"{enemyData.EnemyName}의 화상 효과가 종료되었습니다.");
                // 필요하다면 화상 아이콘 등 UI 제거 로직 추가
            }
            else
            {
                Debug.Log($"{enemyData.EnemyName}의 화상 효과 남은 턴: {burnTurnsRemaining}");
            }
        }
    }
    // ▼▼▼ 출혈 효과 관련 메소드 추가 ▼▼▼
    /// <summary>
    /// 적에게 출혈 효과를 적용합니다.
    /// </summary>
    /// <param name="totalDamage">총 출혈 데미지</param>
    /// <param name="duration">출혈 지속 턴 수</param>
    public void ApplyBleed(int totalDamage, int duration)
    {
        if (duration <= 0) return; // 지속시간이 0 이하면 효과 없음

        // 중첩 방식은 기획에 따라 결정 (현재는 새로 적용된 효과로 덮어쓰기)
        bleedDamagePerTurn = Mathf.CeilToInt((float)totalDamage / duration); // 턴당 피해량 계산 (나누어 떨어지지 않을 경우 올림 처리)
        bleedTurnsRemaining = duration;
        Debug.Log($"{enemyData.EnemyName}에게 {duration}턴 동안 매 턴 {bleedDamagePerTurn}의 출혈 효과가 부여되었습니다. (총 {totalDamage} 피해)");
    }

    /// <summary>
    /// 턴 시작 시 출혈 피해를 처리합니다.
    /// </summary>
    private void ProcessBleedAtTurnStart()
    {
        if (bleedTurnsRemaining > 0)
        {
            Debug.Log($"{enemyData.EnemyName}이(가) 출혈로 {bleedDamagePerTurn}의 직접 피해를 입습니다.");
            TakeDirectDamage(bleedDamagePerTurn); // 방어력 무시 직접 피해
            bleedTurnsRemaining--;

            if (bleedTurnsRemaining <= 0)
            {
                bleedDamagePerTurn = 0; // 출혈 효과 종료
                Debug.Log($"{enemyData.EnemyName}의 출혈 효과가 종료되었습니다.");
            }
            else
            {
                Debug.Log($"{enemyData.EnemyName}의 출혈 효과 남은 턴: {bleedTurnsRemaining}");
            }
        }
    }
    // ▲▲▲ 출혈 효과 관련 메소드 추가 ▲▲▲


    // ---------------------------------
    /// <summary>
    /// 현재 체력 백분율(0~1)을 반환합니다.
    /// </summary>
    public float GetCurrentHpPercentage()
    {
        return (float)currentHp / enemyData.HP;
    }

    //독 수치를 증가시키는 배율 효과 (예: 독을 2배 증가)
    public void BoostPoison(int multiplier)
    {
        poisonStack *= multiplier;
        Debug.Log($"{enemyData.EnemyName}의 독 수치가 {multiplier}배 증가하여 {poisonStack}이 되었습니다.");
    }

    //적에게 디버프를 적용하는 메서드 (예: 저주나 약화)
    public void ApplyDebuff(string debuffName, int duration)
    {
        Debug.Log($"{enemyData.EnemyName}에게 {debuffName} 디버프가 {duration}턴 동안 적용되었습니다.");
        // 실제 디버프 효과 구현은 별도 로직 추가 필요
    }

} // Enemy 클래스의 끝

// 아래의 CharacterIndent, IndentData, EIndent 클래스와 열거형은 별도로 둘 수 있지만,
// Enemy.cs 파일 내에 포함시키고 싶다면 네임스페이스를 사용하거나, 별도의 파일로 분리하는 것이 좋습니다.

public class CharacterIndent
{
    public void AddIndent(IndentData data, int amount)
    {
        Debug.Log($"Indent '{data.name}'를 {amount}만큼 추가했습니다.");
    }
}

[System.Serializable]
public class IndentData
{
    public string name;
}

public enum EIndent
{
    Weak,
    Weakening,
    // 추가 인덴트 타입...
} 
