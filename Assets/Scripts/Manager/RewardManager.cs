//using System.Collections.Generic;
//using UnityEngine;
//using UnityEngine.UI;

//public class RewardManager : MonoBehaviour
//{
//    public static RewardManager Instance { get; private set; }

//    [Header("보상 UI 구성 요소")]
//    [SerializeField] private GameObject rewardPanel;       // 전체 보상 패널 (메인 패널)
//    [SerializeField] private Button goldRewardButton;        // 골드 보상 버튼
//    [SerializeField] private Button cardRewardButton;        // 카드 보상 버튼

//    [Header("카드 보상 서브 패널")]
//    [SerializeField] private GameObject cardRewardPanel;     // 카드 보상 서브 패널 (별도 패널)
//    [SerializeField] private Transform cardRewardContainer;  // 보상 카드 옵션들이 배치될 부모 Transform
//    [SerializeField] private GameObject rewardCardPrefab;    // 보상 카드 프리팹 (RewardCard로 이름 변경)

//    [Header("보상 설정")]
//    [SerializeField] private int goldRewardAmount = 50;        // 지급할 골드 양
//    [SerializeField] private int cardRewardOptionCount = 3;      // 카드 보상 옵션 수

//    // candidateCards 대신 ItemSO.Instance.items를 사용할 것이므로, 따로 candidateCards를 선언하지 않습니다.
//    // 단, 보상 카드 후보로 사용할 아이템들은 ItemSO에서 rarity가 "common" 또는 "rare"인 카드들이어야 합니다.

//    // 보상 지급 여부 체크 (골드와 카드 보상이 모두 지급되어야 패널을 닫음)
//    private bool goldClaimed = false;
//    private bool cardClaimed = false;
//    // 카드 보상 선택 여부 플래그
//    private bool cardChosen = false;

//    private void Awake()
//    {
//        if (Instance == null)
//            Instance = this;
//        else
//            Destroy(gameObject);

//        rewardPanel.SetActive(false);
//        cardRewardPanel.SetActive(false); // 카드 보상 서브 패널 처음에 비활성화
//    }

//    /// <summary>
//    /// 전투 종료 후 보상 UI를 표시합니다.
//    /// </summary>
//    public void ShowRewardPanel()
//    {
//        // 보상 지급 여부 초기화
//        goldClaimed = false;
//        cardClaimed = false;
//        cardChosen = false;
        

//        // 골드 보상 UI 설정 및 활성화
//        ShowGoldReward();

//        // 카드 보상 버튼의 리스너 설정: 클릭 시 카드 보상 서브 패널을 엽니다.
//        cardRewardButton.onClick.RemoveAllListeners();
//        cardRewardButton.onClick.AddListener(() =>
//        {
//            ShowCardRewardPanel();
//        });

//        rewardPanel.SetActive(true);
//    }

//    /// <summary>
//    /// 골드 보상 버튼을 설정합니다.
//    /// </summary>
//    void ShowGoldReward()
//    {
//        goldRewardButton.onClick.RemoveAllListeners();
//        goldRewardButton.onClick.AddListener(() =>
//        {
//            Player.Instance.AddGold(goldRewardAmount);
//            Debug.Log($"{goldRewardAmount} 골드가 지급되었습니다.");
//            goldClaimed = true;
//            CheckAllRewardsClaimed();
//        });
//    }

//    /// <summary>
//    /// 카드 보상 서브 패널을 열어, 후보 카드 옵션 버튼들을 생성합니다.
//    /// ItemSO.Instance.items에서 rarity가 "common" 또는 "rare"인 카드들을 대상으로 60%/40% 확률로 선택합니다.
//    /// </summary>
//    void ShowCardRewardPanel()
//    {
//        // 1. CardDatabase에서 rarity가 "common" 또는 "rare"인 카드들만 필터링
//        List<CardData> filteredCardData = CardDatabase.Instance.cardList.FindAll(cd =>
//        {
//            if (string.IsNullOrEmpty(cd.rarity))
//                return false;
//            string r = cd.rarity.ToLower();
//            return (r == "common" || r == "rare");
//        });

//        // 2. 필터링한 CSV 데이터의 cardName과 일치하는 ItemSO의 Item을 찾아 validRewardItems 리스트 구성
//        List<Item> validRewardItems = new List<Item>();
//        foreach (CardData cd in filteredCardData)
//        {
//            Item foundItem = ItemSO.Instance.GetItemByName(cd.cardName); 
//            if (foundItem != null)
//            {
//                validRewardItems.Add(foundItem);
//            }
//        }

//        // 3. validRewardItems를 rarity별로 분리 (common, rare)
//        List<Item> commonItems = validRewardItems.FindAll(item =>
//            item != null &&
//            !string.IsNullOrEmpty(item.rarity) &&
//            item.rarity.ToLower() == "common"
//        );
//        List<Item> rareItems = validRewardItems.FindAll(item =>
//            item != null &&
//            !string.IsNullOrEmpty(item.rarity) &&
//            item.rarity.ToLower() == "rare"
//        );

//        // 4. 카드 보상 옵션 3개 선택 (각 옵션마다 60% 확률로 common, 40%로 rare)
//        List<Item> rewardOptions = new List<Item>();
//        for (int i = 0; i < cardRewardOptionCount; i++)
//        {
//            float chance = Random.value; // 0.0 ~ 1.0
//            bool chooseCommon = chance < 0.6f;
//            List<Item> pool = chooseCommon ? commonItems : rareItems;
//            if (pool.Count == 0)
//            {
//                // 해당 희귀도 카드가 없다면 validRewardItems 전체에서 선택
//                pool = validRewardItems;
//            }
//            int index = Random.Range(0, pool.Count);
//            rewardOptions.Add(pool[index]);
//            // 중복 선택을 방지하려면 pool에서 제거
//            pool.RemoveAt(index);
//        }

//        // 5. 각 옵션에 대해 RewardCard 프리팹 인스턴스 생성 및 설정
//        foreach (Item item in rewardOptions)
//        {
//            GameObject buttonObj = Instantiate(rewardCardPrefab, cardRewardContainer);
//            RewardCard rewardCard = buttonObj.GetComponent<RewardCard>();
//            rewardCard.Setup(item, OnCardRewardSelected);
//        }

//        // 6. 카드 보상 서브 패널 활성화
//        cardRewardPanel.SetActive(true);
//    }


//    /// <summary>
//    /// 카드 보상 옵션 버튼을 클릭했을 때 호출되는 콜백.
//    /// 선택된 카드가 플레이어 덱에 추가됩니다.
//    /// </summary>
//    void OnCardRewardSelected(Item selectedCard)
//    {
//        // 보상 카드가 선택되면 플레이어 덱에 추가 (예: CardManager.Inst.AddRewardCardToDeck(selectedCard);)
//        CardManager.Inst.AddRewardCardToDeck(selectedCard);
//        Debug.Log($"{selectedCard.name} 카드가 플레이어 덱에 추가되었습니다.");
//        cardClaimed = true;
//        cardChosen = true;
//        // 카드 보상 서브 패널 닫기
//        cardRewardPanel.SetActive(false);
//        CheckAllRewardsClaimed();
//    }

//    /// <summary>
//    /// 두 보상이 모두 수령되었으면 보상 패널을 비활성화합니다.
//    /// </summary>
//    void CheckAllRewardsClaimed()
//    {
//        if (goldClaimed && cardClaimed)
//        {
//            rewardPanel.SetActive(false);
//        }
//    }
//}
