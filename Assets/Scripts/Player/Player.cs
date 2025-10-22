using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.SceneManagement;
using TMPro;
using Spine.Unity;
using Spine;


public class Player : MonoBehaviour
{
    public static Player Instance { get; private set; }

    [Header("체력 관련")]
    [SerializeField] private int maxHealth = 100;
    private int currentHealth;
    public int CurrentHealth => currentHealth;
    public int MaxHealth => maxHealth;
    [Header("방어도 관련")]
    private int currentDefense = 0;

    // ▼▼▼ [이 줄을 추가해주세요] ▼▼▼
    public int CurrentDefense => currentDefense; // 외부에서 현재 방어도를 읽을 수 있게 해주는 속성
    // ▲▲▲ 추가 완료 ▲▲▲

    [Header("공격 관련")]
    [SerializeField] private int baseAttackPower = 0; // '기본' 공격력
    private int attackPower; // 현재 공격력 (버프 등으로 변동 가능)
    public int AttackPower => attackPower;

    [Header("마나 관련")]
    [SerializeField] private int maxMana = 4;
    private int currentMana = 4;
    public int CurrentMana => currentMana;

    [Header("골드 관련")]
    [SerializeField] private int gold = 0;
    public int Gold => gold;

    [Header("이펙트 위치 오프셋")]
    [Tooltip("캐릭터 위치 기준으로 이펙트가 생성될 상대적 위치")]
    public Vector3 effectOffset = new Vector3(1.5f, 1.0f, 0); // 예시: 오른쪽으로 1.5, 위로 1.0

    // 추가 효과를 위한 변수들
    private bool isDoubleDamageTurn = false;
    private int invincibleTurnCount = 0;

    //[Header("오디오 및 UI 프리팹")]
    //[SerializeField] private AudioClip atksound;
    //[SerializeField] private AudioClip defsound;
    //[SerializeField] private AudioClip diesound;

    [SerializeField] private HpBarController playerHpBarPrefab;


    // 내부 참조 변수들
    private HpBarController hpBarController;
    private SkeletonAnimation skeletonAnimation;
    //private AudioSource audioSource;

    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
            Debug.Log("[Player.Awake] Player 인스턴스 설정 및 DontDestroyOnLoad 적용됨.");

            // 최초 실행 시 현재 공격력을 기본 공격력으로 설정
            attackPower = baseAttackPower;
        }
        else if (Instance != this)
        {
            Debug.LogWarning($"[Player.Awake] 다른 Player 인스턴스가 이미 존재하여 이 인스턴스('{this.gameObject.name}')를 파괴합니다.");
            Destroy(gameObject);
            return;
        }

        //audioSource = GetComponent<AudioSource>();
        //if (audioSource == null) audioSource = gameObject.AddComponent<AudioSource>();
        //audioSource.playOnAwake = false;
    }

    void OnEnable()
    {
        UIManager.OnUIManagerReady += UpdateAllUI;
        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    void OnDisable()
    {
        UIManager.OnUIManagerReady -= UpdateAllUI;
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        Debug.Log($"[Player.OnSceneLoaded] 새로운 씬 '{scene.name}' 로드됨. HP 바 재생성 여부 확인.");
        if (scene.name.Contains("Battle"))
        {
            CreateHpBar();
        }
    }

    void Start()
    {
        skeletonAnimation = GetComponent<SkeletonAnimation>();
        if (skeletonAnimation == null)
        {
            Debug.LogError("Player 오브젝트에 SkeletonAnimation 컴포넌트가 없습니다!");
        }
        else
        {
            // 게임 시작 시 기본 상태를 'idle' 애니메이션으로 설정 (무한 반복)
            skeletonAnimation.AnimationState.SetAnimation(0, "idle", true);
        }
        // 체력은 Start에서 한 번만 초기화하여 유지되도록 함
        currentHealth = maxHealth;

        // 새 전투마다 초기화될 스탯들
        PrepareForNewBattle();

        if (SceneManager.GetActiveScene().name.Contains("Battle"))
        {
            CreateHpBar();
        }
        UpdateAllUI();
    }

    /// <summary>
    /// 새로운 전투 시작을 위해 플레이어의 상태를 초기화합니다.
    /// 체력(currentHealth)과 골드(gold)는 유지됩니다.
    /// </summary>
    public void PrepareForNewBattle()
    {
        Debug.Log("[Player] 새 전투를 위해 스탯을 초기화합니다.");

        currentDefense = 0;
        attackPower = baseAttackPower;
        //currentMana = maxMana;

        isDoubleDamageTurn = false;
        invincibleTurnCount = 0;

        UpdateAllUI();
    }

    private void UpdateAllUI()
    {
        if (UIManager.Instance != null)
        {
            Debug.Log($"[Player] UIManager에 UI 업데이트 요청. Gold: {gold}, HP: {currentHealth}, DEF: {currentDefense}");
            UIManager.Instance.UpdateGoldDisplay(gold);

            if (hpBarController != null)
            {
                hpBarController.SetMaxHP(maxHealth);
                hpBarController.SetCurrentHP(currentHealth);
                hpBarController.SetCurrentDefense(currentDefense);
            }
        }
    }

    private void CreateHpBar()
    {
        if (hpBarController != null)
        {
            Destroy(hpBarController.gameObject);
        }

        if (playerHpBarPrefab != null)
        {
            Canvas canvas = FindObjectOfType<Canvas>();
            if (canvas != null)
            {
                GameObject hpBarInstance = Instantiate(playerHpBarPrefab.gameObject, canvas.transform);
                hpBarController = hpBarInstance.GetComponent<HpBarController>();
                if (hpBarController != null)
                {
                    hpBarController.SetTarget(transform);
               
                    UpdateAllUI();
                }
            }
        }
    }

    public void AddGold(int amount)
    {
        if (amount <= 0) return;
        gold += amount;
        UpdateAllUI();
    }

    public bool SpendGold(int amount)
    {
        if (gold >= amount)
        {
            gold -= amount;
            UpdateAllUI(); // 골드 UI 업데이트를 위해 호출
            Debug.Log($"{amount} 골드 사용. 남은 골드: {gold}");
            return true; // 성공
        }
        else
        {
            Debug.Log("골드가 부족하여 사용할 수 없습니다.");
            return false; // 실패
        }
    }
    public void Heal(int amount)
    {
        currentHealth = Mathf.Min(currentHealth + amount, maxHealth);
        UpdateAllUI();
    }

    public void TakeDamage(int damage)
    {
        if (invincibleTurnCount > 0)
        {
            Debug.Log("무적 상태로 인해 피해를 받지 않습니다.");
            return;
        }

        // ▼▼▼ 플레이어 피격 이펙트 생성 코드 추가 ▼▼▼
        if (Player.Instance != null)
        {
            //Vector3 playerHitPos = Player.Instance.transform.position + Player.Instance.effectOffset;
            EffectManager.Instance.PlayEffect(EffectType.PlayerHit, Player.Instance.transform.position);
        }
        // ▲▲▲ 추가 완료 ▲▲▲

        // ▼▼▼ 여기에 적 공격 사운드 재생 코드를 추가합니다. ▼▼▼
        SoundManager.Instance.PlaySfx(SfxType.EnemyAttack);
        // ▲▲▲ 추가 완료 ▲▲▲

        int damageToShield = Mathf.Min(currentDefense, damage);
        currentDefense -= damageToShield;
        int damageToHealth = damage - damageToShield;
        currentHealth -= damageToHealth;

        if (damageToShield > 0) Debug.Log($"방어도가 {damageToShield}만큼의 피해를 막아줬습니다. 남은 방어도: {currentDefense}");
        if (damageToHealth > 0) Debug.Log($"플레이어가 {damageToHealth}의 피해를 입었습니다. 남은 체력: {currentHealth}");

        UpdateAllUI();

        if (currentHealth <= 0) Die();
    }

    public void IncreaseDefense(int defense)
    {
        //if (defsound != null) audioSource.PlayOneShot(defsound);
        skeletonAnimation.AnimationState.SetAnimation(0, "attack02", false);
        skeletonAnimation.AnimationState.AddAnimation(0, "idle", true, 0);
        currentDefense += defense;
        UpdateAllUI();
    }

    /// <summary>
    /// 방어도를 0으로 초기화하고 UI를 업데이트합니다.
    /// </summary>
    public void ResetDefense()
    {
        currentDefense = 0;
        Debug.Log("[Player] 턴 시작 시 방어도를 초기화합니다.");
        UpdateAllUI(); // HP 바의 방어 수치를 갱신하기 위해 호출
    }
 
    public void IncreaseAttack(float amount)
    {
        attackPower += (int)amount;
    }

    public void RefillMana()
    {
        currentMana = maxMana;
        UpdateAllUI();
    }

    public void PerformMultiHit(IEnumerable<Enemy> targets, int damagePerHit, int numberOfHits, float delay)
    {
        StartCoroutine(MultiHitCoroutine(targets, damagePerHit, numberOfHits, delay));
    }

    private IEnumerator MultiHitCoroutine(IEnumerable<Enemy> targets, int damagePerHit, int numberOfHits, float delay)
    {
        AtkAni();
        List<Enemy> targetList = targets.ToList();
        for (int i = 0; i < numberOfHits; i++)
        {
            AtkAni();
            foreach (Enemy target in targetList)
            {
                if (target != null && target.currentHealth > 0)
                {
                    target.Hit(damagePerHit, this);
                }
            }
            if (i < numberOfHits - 1)
            {
                yield return new WaitForSeconds(delay);
            }
        }
    }

    public void SetInvincibleTurn(int turns)
    {
        invincibleTurnCount = turns;
    }

    public void Die()
    {
        SoundManager.Instance.PlaySfx(SfxType.PlayerDie);
        Debug.Log("플레이어가 사망했습니다.");

        // 죽음 애니메이션 재생 (반복 안 함)
        if (skeletonAnimation != null)
        {
            skeletonAnimation.AnimationState.SetAnimation(0, "dead", false);
        }

        // 여기에 게임 오버 처리 로직 추가 (예: 결과창 표시)
    }

    public void AtkAni()
    {
        Debug.Log("### AtkAni 함수 호출됨! ###"); // <<< 확인용 로그 추가
        // SkeletonAnimation 컴포넌트가 없으면 아무것도 하지 않음
        if (skeletonAnimation == null) return;

        // 공격 사운드 재생
        Invoke("PlayAttackSound", 0.3f);

        // 1. "attack1" 애니메이션을 1번만 재생합니다 (반복 안 함: false).
        skeletonAnimation.AnimationState.SetAnimation(0, "attack01", false);

        // 2. "attack1"이 끝난 후에 이어서 "idle" 애니메이션을 재생하도록 예약(큐에 추가)합니다 (무한 반복: true).
        skeletonAnimation.AnimationState.AddAnimation(0, "idle", true, 0);
    }


    /// <summary>
    /// 방어도를 무시하고 체력에 직접 피해를 줍니다. (자해, 고정 피해 등)
    /// </summary>
    public void TakePureDamage(int damage)
    {
        if (invincibleTurnCount > 0)
        {
            Debug.Log("플레이어가 무적 상태라 피해를 입지 않았습니다.");
            return;
        }

        currentHealth -= damage;
        Debug.Log($"<color=purple>플레이어가 {damage}의 순수 피해를 입었습니다. 남은 체력: {currentHealth}</color>");

        // UI 업데이트 및 사망 처리
        if (hpBarController != null) hpBarController.SetCurrentHP(currentHealth);
        if (currentHealth <= 0) Die();
    }
    private void PlayAttackSound()
    {
        // SoundManager를 통해 플레이어 공격 효과음을 재생합니다.
        SoundManager.Instance.PlaySfx(SfxType.PlayerAttack);
    }
}
