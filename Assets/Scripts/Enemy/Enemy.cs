using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using Spine.Unity; // Spine 애니메이션 사용을 위해 추가


//적 행동 타입 enum 정의
public enum EnemyActionType { Attack, Defend, Buff, Debuff } 


public class Enemy : MonoBehaviour
{
    private EnemySO enemyData;
    public EnemySO EnemyData => enemyData; // 공개 읽기 전용 속성

    [Header("적 패턴 UI")]
    [SerializeField] private GameObject intentUIPrefab; //IntentUI 프리팹 연결
    private EnemyIntentUI intentUIInstance;

    //다음 행동 저장을 위한 변수 추가
    private EnemyActionSO nextAction;

    //턴 카운터 변수 추가
    private int turnCounter = 0;

    public CharacterIndent CharacterIndent { get; private set; }

    private int currentHp; //현재 체력
    public int currentHealth => currentHp; //외부에서 현재 체력을 읽을 수 있도록 public 속성 추가

    //방어도 관련 변수                                
    private int currentArmor = 0;

    private HpBarController hpBarController; // HPBarController 참조 변수 추가

    // 독 효과를 위한 변수 (현재 독 스택)
    private int poisonStack = 0;

    //화상 효과 관련 변수 추가 
    private int burnDamagePerTurn = 0;
    private int burnTurnsRemaining = 0;

    //출혈 효과 관련 변수 추가
    private int bleedDamagePerTurn = 0;
    private int bleedTurnsRemaining = 0;

    /// <summary>
    /// 이 적이 현재 화상 상태인지 여부를 반환합니다. (읽기 전용)
    /// </summary>
    public bool IsBurning => burnTurnsRemaining > 0;

    private SkeletonAnimation skeletonAnimation; // Spine 애니메이션 컴포넌트 참조

    /// <summary>
    /// 이 적의 현재 방어도를 반환합니다. (읽기 전용)
    /// </summary>
    public int CurrentArmor => currentArmor;

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


        if (intentUIPrefab != null)
        {
            // "UI_Foreground" 라는 이름의 Canvas를 찾습니다.
            GameObject canvasObj = GameObject.Find("UI_Foreground");
            if (canvasObj != null)
            {
                GameObject uiObj = Instantiate(intentUIPrefab, canvasObj.transform);
                intentUIInstance = uiObj.GetComponent<EnemyIntentUI>();
                intentUIInstance.SetTarget(this.transform);
                intentUIInstance.Hide();
            }
            else
            {
                Debug.LogError("'UI_Foreground' Canvas를 씬에서 찾을 수 없습니다! 의도 UI를 생성할 수 없습니다.");
            }
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
            hpBarController.SetCurrentDefense(currentArmor);
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
        int finalDamage = damage;

        // 방어도가 있다면 먼저 피해량에서 차감
        if (currentArmor > 0)
        {
            int damageToArmor = Mathf.Min(currentArmor, finalDamage);
            currentArmor -= damageToArmor;
            finalDamage -= damageToArmor;
            Debug.Log($"{enemyData.EnemyName}의 방어도가 {damageToArmor}만큼의 피해를 막았습니다. 남은 방어도: {currentArmor}");
            if (hpBarController != null) hpBarController.SetCurrentDefense(currentArmor);
        }

        // 고정 방어력(Defense)을 고려한 실제 체력 피해량 계산
        int effectiveDamage = Mathf.Max(finalDamage - enemyData.Defense, 0);
        currentHp -= effectiveDamage;
        if (effectiveDamage > 0)
        {
            Debug.Log($"{enemyData.EnemyName}에게 {effectiveDamage}의 체력 피해를 입혔습니다. 남은 체력: {currentHp}");

           
            // 실제 입힌 체력 피해량을 BattleContext에 기록합니다.
            BattleContext.TotalDamageDealtThisAction += effectiveDamage;
        }
        if (hpBarController != null)
            hpBarController.SetCurrentHP(currentHp);

        if (currentHp <= 0)
            Die();

        // 실제 입힌 체력 피해량을 반환
        return effectiveDamage;
    }

    //방어도 획득 함수 추가
    public void GainArmor(int amount)
    {
        currentArmor += amount;
        Debug.Log($"{enemyData.EnemyName}이(가) 방어도를 {amount}만큼 얻었습니다. 현재 방어도: {currentArmor}");
        if (hpBarController != null)
        {
            hpBarController.SetCurrentDefense(currentArmor);
        }
        else
        {
            Debug.LogError("[Enemy.GainArmor] hpBarController가 NULL입니다! HP바가 Enemy에 제대로 연결되지 않았습니다.");
        }
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
        // 적 패턴 UI 파괴 로직 추가
        if (intentUIInstance != null)
        {
            Destroy(intentUIInstance.gameObject);
        }
        Destroy(gameObject);
    }

    /// <summary>
    /// 다음 턴에 할 행동을 자신의 패턴 목록에서 랜덤하게 결정합니다.
    /// </summary>
    public void ChooseNextAction()
    {
        if (enemyData == null || enemyData.actionPatterns == null || enemyData.actionPatterns.Count == 0)
        {
            Debug.LogError($"{gameObject.name}에게 설정된 행동 패턴이 없습니다.");
            nextAction = null;
            return;
        }

        // 행동 패턴 리스트에서 무작위로 하나를 선택
        int randomIndex = Random.Range(0, enemyData.actionPatterns.Count);
        nextAction = enemyData.actionPatterns[randomIndex];
        Debug.Log($"{gameObject.name}의 다음 행동 결정: {nextAction.name}");

       
        UpdateIntentUI();
    }


    public void UpdateIntentUI()
    {
        if (intentUIInstance == null) return;

        if (nextAction == null)
        {
            intentUIInstance.Hide();
            return;
        }

        int displayValue = 0;
        if (nextAction.actionType == EnemyActionType.Attack)
        {
            // 공격 행동일 경우, 대략적인 피해량을 계산해서 전달
            displayValue = enemyData.Damage; // 여기서는 평균값인 기본 데미지를 표시
        }
        else if (nextAction.actionType == EnemyActionType.Defend)
        {
            displayValue = enemyData.Defense * 3;
        }

        intentUIInstance.ShowIntent(nextAction, displayValue);
    }


    public IEnumerator PerformTurn()
    {
        Debug.Log($"{enemyData.EnemyName}의 턴 시작. (이전 턴: {turnCounter})");

        // ▼▼▼ 턴 카운터 증가를 맨 위로 이동 ▼▼▼
        // 턴이 시작되면 어떤 행동을 하든 먼저 카운트를 올립니다.
        turnCounter++;
        Debug.Log($"현재 턴 카운트: {turnCounter}");

        // 1. 이 적이 Elite 타입인지 확인
        if (enemyData.Category == EnemyType.Elite)
        {
            // 2. 3턴 주기로 특수 패턴을 발동
            if (turnCounter > 0 && turnCounter % 3 == 0)
            {
                Debug.Log($"{enemyData.EnemyName}이(가) 3턴 패턴 발동! '손실의 룬'을 부여합니다.");

                if (enemyData.penaltyRune != null && RuneDeckManager.Instance != null)
                {
                    RuneDeckManager.Instance.AddRuneToHand(enemyData.penaltyRune);
                }

                // 특수 패턴 후 턴 종료 (이제 turnCounter는 이미 증가했으므로 안전합니다)
                yield break;
            }
        }

        // --- 이하 일반 행동 로직 ---

        // 턴 시작 시 방어도 초기화
        currentArmor = 0;
        if (hpBarController != null) hpBarController.SetCurrentDefense(currentArmor);

        // 턴 시작 시 상태 이상 처리
        ProcessPoisonAtTurnStart();
        if (currentHp <= 0) yield break;

        ProcessBurnAtTurnStart();
        if (currentHp <= 0) yield break;

        ProcessBleedAtTurnStart();
        if (currentHp <= 0) yield break;

        // 일반 행동 실행
        if (nextAction != null)
        {
            Debug.Log($"{gameObject.name}이(가) '{nextAction.name}' 행동을 실행합니다.");
            switch (nextAction.actionType)
            {
                case EnemyActionType.Attack:
                    int randomDamage = Random.Range(1, enemyData.Damage + 1);
                    yield return StartCoroutine(AttackPlayer(randomDamage, nextAction.hitCount));
                    break;
                case EnemyActionType.Defend:
                    int armorToGain = enemyData.Defense * 3;
                    GainArmor(armorToGain);
                    yield return new WaitForSeconds(1f);
                    break;
            }
        }
        else
        {
            Debug.LogWarning($"{gameObject.name}에게 실행할 행동(nextAction)이 결정되지 않았습니다.");
            yield return new WaitForSeconds(1f);
        }

        // 턴 종료 시 처리
        ProcessPoisonAtTurnEnd();
        ChooseNextAction();

        Debug.Log($"{enemyData.EnemyName}의 턴 종료");
        yield return null;
    }



    // ▼▼▼ 이 함수를 아래의 새 코드로 교체해주세요 ▼▼▼
    private IEnumerator AttackPlayer(int damage, int hitCount)
    {
        // 1. 공격 애니메이션을 한 번만 재생합니다. (애니메이션 자체가 여러 번 때리는 모션일 수 있음)
        if (skeletonAnimation != null)
        {
            // "attack" 애니메이션을 반복 없이 재생합니다.
            skeletonAnimation.AnimationState.SetAnimation(0, "attack", false);
        }

        // 약간의 선딜레이 후 실제 타격이 시작되도록 합니다.
        yield return new WaitForSeconds(0.3f); // 이 시간은 애니메이션에 맞춰 조절 가능

        // 2. hitCount 만큼 반복하여 피해를 줍니다.
        for (int i = 0; i < hitCount; i++)
        {
            // 플레이어가 살아있는지 확인
            if (Player.Instance == null || Player.Instance.CurrentHealth <= 0)
            {
                Debug.Log("플레이어가 사망하여 공격을 중단합니다.");
                break; // 플레이어가 죽었으면 반복 중단
            }

            // 실제 공격 (데미지 처리)
            Player.Instance.TakeDamage(damage);
            Debug.Log($"{enemyData.EnemyName}이(가) 플레이어에게 {damage}의 데미지를 입혔습니다. ({i + 1}/{hitCount})");

            // 마지막 타격이 아니라면, 다음 타격 전까지 잠시 대기하여 타격감을 줍니다.
            if (i < hitCount - 1)
            {
                yield return new WaitForSeconds(0.2f); // 연속 타격 사이의 간격
            }
        }

        // 모든 타격이 끝난 후, 애니메이션이 끝날 때까지 잠시 더 기다려 자연스럽게 만듭니다.
        yield return new WaitForSeconds(0.3f); // 애니메이션 후딜레이

        // 3. 공격 후 "idle" 애니메이션으로 전환합니다.
        if (skeletonAnimation != null)
        {
            skeletonAnimation.AnimationState.SetAnimation(0, "idle", true);
        }
    }


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
