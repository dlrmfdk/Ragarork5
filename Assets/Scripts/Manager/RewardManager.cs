using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class RewardManager : MonoBehaviour
{
    public static RewardManager Instance { get; private set; }

    [Header("보상 UI 구성 요소")]
    [SerializeField] private GameObject rewardPanel;       // 전체 보상 패널 (메인 패널)
    [SerializeField] private Button goldRewardButton;        // 골드 보상 버튼
    [SerializeField] private Button cardRewardButton;        // 카드 보상 버튼

    [Header("카드 보상 서브 패널")]
    [SerializeField] private GameObject cardRewardPanel;     // 카드 보상 서브 패널 (별도 패널)
    [SerializeField] private Transform cardRewardContainer;  // 보상 카드 옵션들이 배치될 부모 Transform
    [SerializeField] private GameObject rewardCardPrefab;    // 보상 카드 프리팹 (RewardCard, CardUI 역할)

    [Header("보상 설정")]
    [SerializeField] private int goldRewardAmount = 50;        // 지급할 골드 양
    [SerializeField] private int cardRewardOptionCount = 3;      // 보상 카드 옵션 수

    [Header("ItemSO 참조")]
    [SerializeField] private ItemSO itemSO;  // 보상 카드 후보로 사용할 ItemSO (각 Item은 rarity 필드가 설정되어 있어야 함)

    // 보상 지급 여부 체크 (골드와 카드 보상이 모두 지급되어야 패널을 닫음)
    private bool goldClaimed = false;
    private bool cardClaimed = false;
    private bool cardChosen = false;

    private void Awake()
    {
        if (Instance == null)
            Instance = this;
        else
            Destroy(gameObject);

        rewardPanel.SetActive(false);
        cardRewardPanel.SetActive(false); // 카드 보상 서브 패널은 처음에 비활성화
    }

    /// <summary>
    /// 전투 종료 후 보상 UI를 표시합니다.
    /// </summary>
    public void ShowRewardPanel()
    {
        // 보상 지급 여부 초기화
        goldClaimed = false;
        cardClaimed = false;
        cardChosen = false;

        // 골드 보상 UI 설정 및 활성화
        ShowGoldReward();

        // 카드 보상 버튼의 리스너 설정: 클릭 시 카드 보상 서브 패널을 엽니다.
        cardRewardButton.onClick.RemoveAllListeners();
        cardRewardButton.onClick.AddListener(() => { ShowCardRewardPanel(); });

        rewardPanel.SetActive(true);
    }

    /// <summary>
    /// 골드 보상 버튼을 설정합니다.
    /// </summary>
    void ShowGoldReward()
    {
        goldRewardButton.onClick.RemoveAllListeners();
        goldRewardButton.onClick.AddListener(() =>
        {
            Player.Instance.AddGold(goldRewardAmount);
            Debug.Log($"{goldRewardAmount} 골드가 지급되었습니다.");
            goldClaimed = true;
            CheckAllRewardsClaimed();
        });
    }

    /// <summary>
    /// 카드 보상 서브 패널을 열어, ItemSO에서 Common, Rare 카드만 대상으로 보상 카드 옵션을 생성합니다.
    /// 각 옵션은 60% 확률로 Common, 40% 확률로 Rare 카드를 선택하며,
    /// RewardCard(즉, CardUI)를 사용하여 카드 정보를 표시하고 DOTween 애니메이션을 적용합니다.
    /// </summary>
    void ShowCardRewardPanel()
    {
        // 기존 보상 카드 옵션 제거 (컨테이너 내 모든 자식 제거)
        foreach (Transform child in cardRewardContainer)
        {
            Destroy(child.gameObject);
        }

        // ItemSO의 카드 중 rarity가 Common 또는 Rare인 카드 필터링
        List<Item> validRewardItems = new List<Item>();
        foreach (Item item in itemSO.items)
        {
            // item.rarity는 Rarity 타입으로 설정되어 있다고 가정합니다.
            if (item != null && (item.rarity == Rarity.Common || item.rarity == Rarity.Rare))
            {
                validRewardItems.Add(item);
            }
        }

        // rarity별로 분리
        List<Item> commonItems = validRewardItems.FindAll(item => item.rarity == Rarity.Common);
        List<Item> rareItems = validRewardItems.FindAll(item => item.rarity == Rarity.Rare);

        // 보상 카드 옵션 선택 (총 cardRewardOptionCount 옵션)
        List<Item> rewardOptions = new List<Item>();
        for (int i = 0; i < cardRewardOptionCount; i++)
        {
            float chance = Random.value; // 0.0 ~ 1.0 사이의 값
            bool chooseCommon = chance < 0.6f;
            List<Item> pool = chooseCommon ? commonItems : rareItems;
            if (pool.Count == 0)
            {
                pool = validRewardItems;
            }
            int index = Random.Range(0, pool.Count);
            rewardOptions.Add(pool[index]);
            // 중복 선택 방지를 위해 선택된 카드를 풀에서 제거
            pool.RemoveAt(index);
        }

        // 각 옵션에 대해 RewardCardPrefab 인스턴스를 생성 및 설정
        foreach (Item item in rewardOptions)
        {
            GameObject cardOption = Instantiate(rewardCardPrefab, cardRewardContainer);
            RewardCard rewardCard = cardOption.GetComponent<RewardCard>();
            // RewardCard의 Setup()에서 Item 정보를 UI에 적용하고 클릭 이벤트를 연결합니다.
            rewardCard.Setup(item, OnCardRewardSelected);
            // DOTween 애니메이션으로 카드 등장 효과 재생 (예: 0.5초)
            rewardCard.PlayShowAnimation(0.5f);
        }

        // 카드 보상 서브 패널 활성화
        cardRewardPanel.SetActive(true);
    }

    /// <summary>
    /// 카드 보상 옵션 버튼을 클릭했을 때 호출되는 콜백.
    /// 선택된 카드가 플레이어 덱에 추가됩니다.
    /// </summary>
    void OnCardRewardSelected(Item selectedCard)
    {
        // 선택된 카드를 플레이어 덱에 추가
        CardManager.Inst.AddRewardCardToDeck(selectedCard);
        Debug.Log($"{selectedCard.name} 카드가 플레이어 덱에 추가되었습니다.");
        cardClaimed = true;
        cardChosen = true;

        // 카드 보상 서브 패널 닫기
        cardRewardPanel.SetActive(false);
        CheckAllRewardsClaimed();
    }

    /// <summary>
    /// 두 보상이 모두 수령되었으면 보상 패널을 비활성화합니다.
    /// </summary>
    void CheckAllRewardsClaimed()
    {
        if (goldClaimed && cardClaimed)
        {
            rewardPanel.SetActive(false);
        }
    }
}
