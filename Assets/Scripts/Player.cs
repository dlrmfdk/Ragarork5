using TMPro;
using UnityEngine;

public class Player : MonoBehaviour
{
    public static Player Instance { get; private set; }

    [Header("방어도(Defense) 관련")]
    [SerializeField] private int currentDefense = 0;

    [Header("체력 관련")]
    [SerializeField]
    private int maxHealth = 100; //최대체력 100
    private int currentHealth; //현재체력

    [Header("공격 관련")]
    [SerializeField] private int attackPower = 10; // 기본 공격력
    // 추가 공격 버프(힘의 축복 등)로 증가시킬 값

    [Header("마나 관련")]
    [SerializeField] private int maxMana = 4; // 최대 마나 4
    private int currentMana = 4;
    public int CurrentMana { get { return currentMana; } }


    [Header("골드 관련")]
    [SerializeField] private int gold = 0; // 초기 골드
    //골드 관련 텍스트 매쉬 프로
    [SerializeField] private TextMeshProUGUI goldText; // 골드 UI 텍스트 (필요 시 사용)

    // 추가 효과를 위한 변수들
    private bool isDoubleDamageTurn = false; // 이번 턴에 공격 데미지 2배 효과
    private int invincibleTurnCount = 0;       // 무적 턴 카운트 (턴 종료마다 감소)


    [Header("UI 관련")]
    [SerializeField] private AudioClip atksound;
    [SerializeField] private AudioClip defsound;
    [SerializeField] private AudioClip diesound;
    [SerializeField] private HpBarController playerHpBarPrefab; // [SerializeField]으로 프리팹 할당

    private HpBarController hpBarController;
    private Animator animator;
    private AudioSource audioSource;

    void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);

        audioSource = GetComponent<AudioSource>();
        if (audioSource == null)
        {
            audioSource = gameObject.AddComponent<AudioSource>();
        }
        audioSource.playOnAwake = false;
    }

    void Start()
    {
        animator = GetComponent<Animator>();
        currentHealth = maxHealth;
        currentMana = maxMana;

        // playerHpBarPrefab을 Canvas 밑에 생성
        if (playerHpBarPrefab != null)
        {
            Canvas canvas = FindObjectOfType<Canvas>();
            if (canvas != null)
            {
                GameObject hpBarInstance = Instantiate(playerHpBarPrefab.gameObject, canvas.transform);
                hpBarInstance.SetActive(true);

                hpBarController = hpBarInstance.GetComponent<HpBarController>();
                if (hpBarController != null)
                {
                    // 타겟(플레이어)를 HPBarController가 따라가도록 설정
                    hpBarController.SetTarget(transform);

                    // 체력 최대/현재값 초기화
                    hpBarController.SetMaxHP(maxHealth);
                    hpBarController.SetCurrentHP(currentHealth);

                    // 방어도 최대/현재값 초기화 (현재는 필요 시)
                    // hpBarController.SetMaxDefense(원하는값);

                    // HP바가 캐릭터 머리 위에 뜨도록 오프셋 조절 (원하는 위치로)
                    hpBarController.SetOffset(new Vector3(0, 180f, 0));
                }
                else
                {
                    Debug.LogError("Player: PlayerHpBarPrefab에 HpBarController가 없습니다.");
                }
            }
            else
            {
                Debug.LogError("Player: 씬에 Canvas가 존재하지 않습니다.");
            }
        }
        else
        {
            Debug.LogError("Player: PlayerHpBarPrefab이 할당되지 않았습니다.");
        }
    }
    // 마나 사용: 사용하려는 비용보다 충분한지 검사 후 차감
    public bool TryUseMana(int cost)
    {
        if (currentMana >= cost)
        {
            currentMana -= cost;
            Debug.Log($"마나 {cost} 사용. 남은 마나: {currentMana}");
            // UI 업데이트 호출 (예: manaBarController.SetCurrentMana(currentMana))
            return true;
        }
        else
        {
            Debug.Log("마나가 부족합니다!");
            return false;
        }
    }
    //턴 시작시 마나 최대치 만큼 충전
    public void RefillMana()
    {
        currentMana = maxMana;
        Debug.Log($"플레이어의 마나가 {maxMana}로 충전되었습니다.");
       
    }


    // 마나 회복 (필요 시)
    public void RecoverMana(int amount)
    {
        currentMana = Mathf.Min(currentMana + amount, maxMana);
        Debug.Log($"마나 {amount} 회복. 현재 마나: {currentMana}");
        // UI 업데이트
    }
    // 플레이어의 최대 마나 증가 메서드
    public void IncreaseMaxMana(int amount)
    {
        maxMana += amount;
        currentMana = Mathf.Min(currentMana, maxMana);
        Debug.Log($"플레이어의 최대 마나가 {amount}만큼 증가하여 현재 최대 마나: {maxMana}");
        // 필요 시 UI 업데이트
    }

    // 플레이어가 카드를 뽑는 메서드 (덱 관리 시스템과 연동)
    public void DrawCards(int count)
    {
        for (int i = 0; i < count; i++)
        {
            // CardManager의 AddCard(true)를 호출하면, 손패에 카드가 추가됩니다.
            //CardManager.Inst.AddCard(true);
        }
        Debug.Log($"플레이어가 {count}장의 카드를 뽑습니다.");
    }

    // 플레이어에게 골드를 추가하는 메서드 (골드 시스템 추가)
    public void AddGold(int amount)
    {
        gold += amount;
        Debug.Log($"플레이어의 골드가 {amount}만큼 증가하였습니다. 현재 골드: {gold}");
        //골드 텍스트 업데이트
        if (goldText != null)
        {
            goldText.text = $"{gold}";
        }
        else
        {
            Debug.LogError("골드 텍스트 UI가 할당되지 않았습니다.");
        }

        // 필요 시 골드 UI 업데이트 호출
    }


    // 플레이어의 덱에 카드 추가 (예: 불꽃의 일격 효과)
    public void AddCardToDeck(string cardName)
    {
        // 실제 구현: CardDatabase.Instance.GetCardDataByName(cardName)로 데이터 얻고, Item 생성 후 DeckManager.Instance.AddCardToDeck(newItem) 호출
        Debug.Log($"{cardName} 카드를 덱에 추가합니다.");
        // 임시 예시:
        // Item newCard = new Item();
        // newCard.name = cardName;
        // newCard.type = ItemType.Attack; // 또는 적절한 타입
        // DeckManager.Instance.AddCardToDeck(newCard);
    }


    // 방어도 증가
    public void IncreaseDefense(int defense)
    {
        audioSource.PlayOneShot(defsound);
        currentDefense += defense;
        Debug.Log($"플레이어가 {defense}만큼 방어도를 얻었습니다. 현재 방어도: {currentDefense}");

        if (hpBarController != null)
        {
            hpBarController.SetCurrentDefense(currentDefense);
        }
    }

    // 체력 피해 처리 (방어도 먼저 소모)
    public void TakeDamage(int damage)
    {
        int finalDamage = damage;

        // 방어도 먼저 감소
        if (currentDefense > 0)
        {
            int usedDef = Mathf.Min(currentDefense, damage);
            currentDefense -= usedDef;
            finalDamage -= usedDef;
            Debug.Log($"방어도가 {usedDef}만큼의 피해를 막아줬습니다. 남은 방어도: {currentDefense}");

            if (hpBarController != null)
            {
                hpBarController.SetCurrentDefense(currentDefense);
            }
        }

        // 만약 무적 턴이 남아있으면 피해를 받지 않음
        if (invincibleTurnCount > 0)
        {
            Debug.Log("무적 상태로 인해 피해를 받지 않습니다.");
            finalDamage = 0;
        }



        currentHealth -= finalDamage;
        Debug.Log($"플레이어가 {finalDamage}의 피해를 입었습니다. 남은 체력: {currentHealth}");

        // HP 바 갱신
        if (hpBarController != null)
            hpBarController.SetCurrentHP(currentHealth);

        if (currentHealth <= 0)
            Die();
    }
    //회복
    public void Heal(int amount)
    {
        currentHealth = Mathf.Min(currentHealth + amount, maxHealth);
        Debug.Log($"플레이어가 {amount}만큼 회복했습니다. 현재 체력: {currentHealth}");
        if (hpBarController != null)
            hpBarController.SetCurrentHP(currentHealth);
    }

    // 공격력 증가 (예: 힘의 축복 효과)
    public void IncreaseAttack(float amount)
    {
        attackPower += (int)amount;
        Debug.Log($"플레이어의 공격력이 {amount}만큼 증가했습니다. 현재 공격력: {attackPower}");
        // 추가로 UI 업데이트가 필요하면 구현
    }

    //// 이번 턴 데미지 2배 효과 (예: 선천진기)
    //public void DoubleDamageThisTurn()
    //{
    //    isDoubleDamageTurn = true;
    //    Debug.Log("이번 턴 공격 데미지가 2배가 됩니다.");
    //    // 턴 종료 시 초기화 로직 필요
    //}

    // 무적 상태 (예: 발할라의 방패) 설정
    public void SetInvincibleTurn(int turns)
    {
        invincibleTurnCount = turns;
        Debug.Log($"플레이어가 {turns}턴 동안 무적 상태가 됩니다.");
    }

    //// 덱 내 공격 카드 개수 세기 (예: 타격의 대가 효과)
    //public int CountAttackCardsInDeck()
    //{
    //    int count = 0;
    //    foreach (var card in deck)
    //    {
    //        if (card.type == ItemType.Attack)
    //            count++;
    //    }
    //    Debug.Log($"덱 내 공격 카드 수: {count}");
    //    return count;
    //}
   


    //// 덱에 카드 추가 (예: 불꽃의 일격 효과)
    //public void AddCardToDeck(string cardName)
    //{
    //    // 카드 생성 방식은 프로젝트에 따라 달라집니다.
    //    // 여기선 단순히 로그 출력 및 임시 추가로 구현합니다.
    //    Debug.Log($"{cardName} 카드를 덱에 추가합니다.");
    //    // deck.Add(new Item(...));  // 실제로 카드 데이터를 생성하여 추가하는 로직 필요
    //}

    //// 플레이어에게 화상 효과 적용 (예: 저주 화상)
    //public void ApplyBurn(int burnDamage)
    //{
    //    Debug.Log($"플레이어가 화상으로 {burnDamage}의 피해를 입습니다.");
    //    TakeDamage(burnDamage);
    //    // 추가로 화상 지속 효과(턴마다 피해 등) 구현 가능
    //}


    public void Die()
    {
        audioSource.PlayOneShot(diesound);
        Debug.Log("플레이어가 사망했습니다.");
        // 게임 오버 처리
    }

    // 공격 애니메이션 예시
    public void AtkAni()
    {
        audioSource.PlayOneShot(atksound);
        animator.SetBool("Attack", true);
        Invoke("ResetAttack", 0.1f);
    }

    private void ResetAttack()
    {
        animator.SetBool("Attack", false);
    }
}
