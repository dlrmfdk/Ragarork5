using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class RewardManager : MonoBehaviour
{
    public static RewardManager Instance { get; private set; }

    [Header("보상 UI 구성 요소")]
    [SerializeField] private GameObject rewardPanel;
    [SerializeField] private Button goldRewardButton;
    [SerializeField] private Button cardRewardButton;

    [Header("카드 보상 서브 패널")]
    [SerializeField] private GameObject cardRewardPanel;
    [SerializeField] private Transform cardRewardContainer;  // 일반 Transform으로 변경 (Panel)
    [SerializeField] private GameObject rewardCardPrefab;      // 보상 카드 프리팹

    [Header("보상 설정")]
    [SerializeField] private int goldRewardAmount = 50;
    [SerializeField] private int cardRewardOptionCount = 3;

    [Header("ItemSO 참조")]
    [SerializeField] private ItemSO itemSO;

    private bool goldClaimed = false;
    private bool cardClaimed = false;
    private bool cardChosen = false;
    private List<GameObject> cachedRewardPanels = new List<GameObject>();
    //private GameObject[] rewardPanels;
    private void Awake()
    {
        if (Instance == null)
            Instance = this;
        else
            Destroy(gameObject);

        rewardPanel.SetActive(false);
        cardRewardPanel.SetActive(false);
        //rewardPanels = GameObject.FindGameObjectsWithTag("RewardPanel");
    }

    public void ShowRewardPanel()
    {
        goldClaimed = false;
        cardClaimed = false;
        cardChosen = false;

        ShowGoldReward();
        cardRewardButton.onClick.RemoveAllListeners();
        cardRewardButton.onClick.AddListener(() => { ShowCardRewardPanel(); });

        rewardPanel.SetActive(true);
    }

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

    void ShowCardRewardPanel()
    {

        // 카드 획득 버튼 숨김
        cardRewardButton.gameObject.SetActive(false);

        //태그가 RewardPanel인 오브젝트 비활성화
        GameObject[] rewardPanels = GameObject.FindGameObjectsWithTag("RewardPanel");
        foreach (GameObject panel in rewardPanels)
        {
            cachedRewardPanels.Add(panel);
            panel.SetActive(false);
        }
        // 보상 카드 후보 필터링
        List<Item> validRewardItems = new List<Item>();
        foreach (Item item in itemSO.items)
        {
            if (item != null && (item.rarity == Rarity.Common || item.rarity == Rarity.Rare))
                validRewardItems.Add(item);
        }

        // rarity별 풀 분리
        List<Item> commonItems = validRewardItems.FindAll(item => item.rarity == Rarity.Common);
        List<Item> rareItems = validRewardItems.FindAll(item => item.rarity == Rarity.Rare);

        // 보상 카드 뽑기 (총 cardRewardOptionCount 옵션)
        List<Item> rewardOptions = new List<Item>();
        for (int i = 0; i < cardRewardOptionCount; i++)
        {
            float chance = Random.value;
            bool chooseCommon = chance < 0.6f;
            List<Item> pool = chooseCommon ? commonItems : rareItems;
            if (pool.Count == 0)
            {
                pool = validRewardItems;
            }
            int index = Random.Range(0, pool.Count);
            rewardOptions.Add(pool[index]);
            pool.RemoveAt(index);
        }

        // 5) 보상 카드 옵션들을 배치하는 함수 호출
        InstantiateRewardCards(rewardOptions);


        // 카드 보상 패널 활성화
        cardRewardPanel.SetActive(true);


    }
    // RewardManager 클래스 내에 추가할 함수
    private void InstantiateRewardCards(List<Item> rewardOptions)
    {
        // 카드 배치 (가운데 정렬)
        // - spacing: 카드 간격 (월드 유닛 또는 UI 단위; 여기서는 예시로 10f 사용)
        // - startX : 첫 카드의 X 좌표 (계산을 통해 가운데 정렬)
        // - posY   : 카드의 Y 위치
        // - scale  : 카드 크기 배율
        float spacing = 10f;
        float startX = -spacing * (rewardOptions.Count - 1) / 2f;
        float posY = 0f;
        float scale = 1f; // 필요에 따라 조절

        for (int i = 0; i < rewardOptions.Count; i++)
        {
            // 보상 카드 프리팹 Instantiate 및 부모(cardRewardContainer) 할당
            GameObject cardObj = Instantiate(rewardCardPrefab, cardRewardContainer);

            // 일반 Transform을 사용하여 카드 위치, 회전, 스케일 설정
            Transform cardTrans = cardObj.transform;
            float posX = startX + spacing * i;
            cardTrans.localPosition = new Vector3(posX, posY, 0f);
            cardTrans.localRotation = Quaternion.identity;
            cardTrans.localScale = Vector3.one * scale;

            // RewardCard 스크립트의 Setup() 호출 (보상 카드 데이터 및 선택 시 호출할 RewardManager의 콜백 연결)
            RewardCard rewardCard = cardObj.GetComponent<RewardCard>();
            rewardCard.Setup(rewardOptions[i], OnCardRewardSelected);
            rewardCard.PlayShowAnimation(0.5f);
        }
    }

    public void OnCardRewardSelected(Item selectedCard)
    {
        // 선택된 카드를 플레이어 덱에 추가
        CardManager.Inst.AddRewardCardToDeck(selectedCard);
        Debug.Log($"{selectedCard.name} 카드가 플레이어 덱에 추가되었습니다.");

        // 보상 카드 선택 처리가 완료되었음을 기록
        cardClaimed = true;
        cardChosen = true;

        // 카드 보상 패널 비활성화
        cardRewardPanel.SetActive(false);

        // 필요 시 추가 후속 처리
        CheckAllRewardsClaimed();
    }

    void CheckAllRewardsClaimed()
    {
        if (goldClaimed && cardClaimed)
        {
            rewardPanel.SetActive(false);

        }
    }
    //클릭 하면 Hierarchy창에 있는 rewardcard 태그들 전부 비활성화 하는 함수
    public void DisableRewardCards()
    {
        GameObject[] rewardCards = GameObject.FindGameObjectsWithTag("RewardCard");
        foreach (GameObject card in rewardCards)
        {
            card.SetActive(false);
        }

        //rewardPanel 활성화
        foreach (GameObject panel in cachedRewardPanels)
        {
            panel.SetActive(true);
        }

    }
}
