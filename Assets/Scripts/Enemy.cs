using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class Enemy : MonoBehaviour
{
    [SerializeField] private EnemySO enemyData;
    public EnemySO EnemyData => enemyData; // 공개 읽기 전용 속성

    public CharacterIndent CharacterIndent { get; private set; }

    private int currentHp; //현재 체력

    // TurnManager 등 외부에서 현재 체력을 읽을 수 있도록 public 속성 추가
    public int currentHealth => currentHp;

    private HpBarController hpBarController; // HPBarController 참조 변수 추가

    // 독 효과를 위한 변수 (현재 독 스택)
    private int poisonStack = 0;

    void Awake()
    {
        CharacterIndent = new CharacterIndent();
    }

    public void Initialize(EnemySO data)
    {
        enemyData = data;
        currentHp = enemyData.HP;

        // 적의 이름을 설정합니다.
        gameObject.name = enemyData.EnemyName;
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

    public void Hit(int damage, Player player)
    {
        // 방어력을 고려한 실제 피해량 계산
        int effectiveDamage = Mathf.Max(damage - enemyData.Defense, 0);
        currentHp -= effectiveDamage;
        Debug.Log($"{enemyData.EnemyName}에게 {effectiveDamage}의 피해를 입혔습니다. 남은 체력: {currentHp}");

        if (hpBarController != null)
            hpBarController.SetCurrentHP(currentHp);
        else
            Debug.LogWarning("HPBarController가 할당되지 않았습니다.");

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

        // 턴 시작 시 독 피해 처리
        ProcessPoisonAtTurnStart();

       
        // 독 피해로 인해 적의 현재체력이 0 이하라면 턴을 종료
        if (currentHp <= 0)
        {
            yield break;
        }
        // 공격 패턴 예시: 플레이어에게 적 스탯의 데미지
        yield return StartCoroutine(AttackPlayer(enemyData.Damage));

        // 턴 종료 시 독 스택 감소 처리
        ProcessPoisonAtTurnEnd();

        Debug.Log($"{enemyData.EnemyName}의 턴 종료");
    }

    private IEnumerator AttackPlayer(int damage)
    {
        yield return new WaitForSeconds(0.5f);
        Player.Instance.TakeDamage(damage);
        Debug.Log($"{enemyData.EnemyName}이 플레이어에게 {damage}의 데미지를 입혔습니다.");
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
