// ShopManager.cs (수정된 최종 버전)
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using System; // Action을 사용하기 위해 필요

public class ShopManager : MonoBehaviour
{
    public static ShopManager Instance { get; private set; }

    [Header("상점 설정")]
    [SerializeField] private List<RuneSO> allSellableRunes;
    [SerializeField] private int numberOfItemsToShow = 6;

    [Header("UI 요소")]
    [SerializeField] private Transform itemContainer;
    [SerializeField] private GameObject shopItemPrefab;
    [SerializeField] private TMP_Text playerGoldText;
    [SerializeField] private Button exitButton;

    // ▼▼▼ 여기에 강화 UI 관련 변수들을 추가합니다. ▼▼▼
    [Header("룬 강화 UI (RewardManager와 동일하게 연결)")]
    [SerializeField] private GameObject enhancementPanel;
    [SerializeField] private List<EnhancementSlotUI> enhancementSlots;
    // ▲▲▲ 변수 추가 완료 ▲▲▲

    // ▼▼▼ 구매/강화 절차에 필요한 임시 변수들을 추가합니다. ▼▼▼
    private RuneSO runeToPurchase; // 구매하려는 상점 룬
    private int purchasePrice; // 해당 룬의 가격
    private ShopItemUI purchasingItemUI; // 클릭한 상점 아이템 UI (구매 완료 처리를 위해)
    // ▲▲▲ 변수 추가 완료 ▲▲▲

    private void Awake()
    {
        if (Instance == null) { Instance = this; }
        else { Destroy(gameObject); }
    }

    private void Start()
    {
        GenerateShopItems();
        UpdatePlayerGoldUI();
        if (enhancementPanel != null) enhancementPanel.SetActive(false); // 시작할 때 강화 패널 숨기기
        // exitButton.onClick.AddListener(...);
    }

    private void GenerateShopItems()
    {
        // ... (기존 코드와 동일) ...
        foreach (Transform child in itemContainer)
        {
            Destroy(child.gameObject);
        }

        var availableRunes = new List<RuneSO>(allSellableRunes);
        var runesToDisplay = new List<RuneSO>();
        int count = Mathf.Min(numberOfItemsToShow, availableRunes.Count);

        for (int i = 0; i < count; i++)
        {
            int randomIndex = UnityEngine.Random.Range(0, availableRunes.Count);
            runesToDisplay.Add(availableRunes[randomIndex]);
            availableRunes.RemoveAt(randomIndex);
        }

        foreach (var rune in runesToDisplay)
        {
            GameObject itemGO = Instantiate(shopItemPrefab, itemContainer);
            // ShopItemUI의 Setup 함수에 ShopManager 자신을 넘겨주도록 수정할 수 있지만,
            // 여기서는 싱글톤을 활용하는 현재 구조를 유지하겠습니다.
            itemGO.GetComponent<ShopItemUI>().Setup(rune);
        }
    }

    public int GetPriceForRarity(RuneRarity rarity)
    {
        // ... (기존 코드와 동일) ...
        switch (rarity)
        {
            case RuneRarity.Common: return 100;
            case RuneRarity.Rare: return 150;
            case RuneRarity.Legend: return 200;
            default: return 999;
        }
    }

    // ▼▼▼ 구매 로직을 완전히 새로 작성합니다. ▼▼▼
    public void AttemptPurchase(RuneSO rune, int price, ShopItemUI itemUI)
    {
        // 1. 골드가 충분한지 확인
        if (Player.Instance.Gold < price)
        {
            Debug.Log("골드가 부족합니다!");
            // 여기에 골드 부족 알림 UI를 띄워줄 수 있습니다.
            return;
        }

        // 2. 강화할 수 있는 같은 색의 기본 룬이 있는지 확인
        var basicRunes = RuneDeckManager.Instance.GetBasicRunesByColor(rune.color);
        if (basicRunes.Count == 0)
        {
            Debug.Log($"강화할 수 있는 {rune.color}색 기본 룬이 없습니다.");
            // 여기에 알림 UI를 띄워줄 수 있습니다.
            return;
        }

        // 3. 모든 조건이 충족되면, 구매 절차 시작
        this.runeToPurchase = rune;
        this.purchasePrice = price;
        this.purchasingItemUI = itemUI;

        // 4. 강화 패널을 보여줌
        ShowShopEnhancementPanel(basicRunes);
    }

    private void ShowShopEnhancementPanel(List<RuneInstance> basicRunes)
    {
        if (enhancementPanel == null || enhancementSlots == null) return;

        // RewardManager의 ShowEnhancementPanel과 거의 동일한 로직
        for (int i = 0; i < enhancementSlots.Count; i++)
        {
            if (i < basicRunes.Count)
            {
                enhancementSlots[i].gameObject.SetActive(true);
                // 슬롯 UI에 룬 정보와, '클릭 시 이 함수를 실행하라'는 명령(콜백)을 함께 전달
                enhancementSlots[i].Setup(basicRunes[i], OnBasicRuneSelectedForPurchase);
            }
            else
            {
                enhancementSlots[i].gameObject.SetActive(false);
            }
        }
        enhancementPanel.SetActive(true);
    }

    // 플레이어가 강화할 기본 룬을 최종 선택했을 때 호출될 함수
    private void OnBasicRuneSelectedForPurchase(RuneInstance chosenBasicRune)
    {
        // 1. 골드 차감
        Player.Instance.SpendGold(this.purchasePrice);

        // 2. 룬 강화 실행
        RuneDeckManager.Instance.EnhanceRune(chosenBasicRune, this.runeToPurchase);

        // 3. UI 처리
        enhancementPanel.SetActive(false); // 강화 패널 닫기
        purchasingItemUI.MarkAsSold(); // 구매한 아이템을 '판매 완료'로 표시
        UpdatePlayerGoldUI(); // 골드 UI 업데이트

        Debug.Log($"{runeToPurchase.displayName}으로 강화 완료!");
    }
    // ▲▲▲ 로직 작성 완료 ▲▲▲

    public void UpdatePlayerGoldUI()
    {
        // ... (기존 코드와 동일) ...
        if (playerGoldText != null && Player.Instance != null)
        {
            playerGoldText.text = $"{Player.Instance.Gold}";
        }
    }
}