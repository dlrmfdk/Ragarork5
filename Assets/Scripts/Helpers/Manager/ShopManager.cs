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

    /// <summary>
    /// (1단계) 룬 구매를 시도하고 강화 재료가 있는지 확인합니다.
    /// </summary>
    public void AttemptPurchase(RuneSO rune, int price, ShopItemUI itemUI)
    {
        // 1. 골드가 충분한지 확인
        if (Player.Instance.Gold < price)
        {
            Debug.Log("골드가 부족합니다!");
            return;
        }

        // ▼▼▼ [수정] 'GetBasicRunesByColor' -> 'GetAllRunesByColor'로 변경 ▼▼▼
        // 2. 강화할 수 있는 같은 색의 '모든 룬'이 있는지 확인 (기본 룬 + 강화 룬 포함)
        var runesToEnhance = RuneDeckManager.Instance.GetAllRunesByColor(rune.color);
        if (runesToEnhance.Count == 0)
        // ▲▲▲ 수정 완료 (변수 이름 변경) ▲▲▲
        {
            Debug.Log($"강화할 수 있는 {rune.color}색 기본 룬이 없습니다.");
            return;
        }

        // 3. 모든 조건이 충족되면, 구매 정보를 임시 변수에 저장하고 강화 절차를 시작
        this.runeToPurchase = rune;
        this.purchasePrice = price;
        this.purchasingItemUI = itemUI;

        // 4. 강화 패널을 보여줌
        ShowShopEnhancementPanel(runesToEnhance);
    }

    /// <summary>
    /// (2단계) 강화할 기본 룬을 선택하는 UI를 엽니다. (RewardManager 로직과 동일)
    /// </summary>
    private void ShowShopEnhancementPanel(List<RuneInstance> basicRunes)
    {
        if (enhancementPanel == null || enhancementSlots == null) return;

        for (int i = 0; i < enhancementSlots.Count; i++)
        {
            if (i < basicRunes.Count)
            {
                enhancementSlots[i].gameObject.SetActive(true);
                // 슬롯 UI에 룬 정보와, '클릭 시 OnBasicRuneSelectedForPurchase 함수를 실행하라'는 명령(콜백)을 함께 전달
                enhancementSlots[i].Setup(basicRunes[i], OnBasicRuneSelectedForPurchase);
            }
            else
            {
                enhancementSlots[i].gameObject.SetActive(false);
            }
        }
        enhancementPanel.SetActive(true);
    }

    /// <summary>
    /// (3단계) 플레이어가 강화할 기본 룬을 최종 선택했을 때 호출될 함수
    /// </summary>
    private void OnBasicRuneSelectedForPurchase(RuneInstance chosenBasicRune)
    {
        // 1. 골드를 차감합니다.
        Player.Instance.SpendGold(this.purchasePrice);

        // 2. RuneDeckManager에 룬 강화를 요청합니다.
        RuneDeckManager.Instance.EnhanceRune(chosenBasicRune, this.runeToPurchase);

        // 3. 모든 UI를 최종 처리합니다.
        enhancementPanel.SetActive(false);      // 강화 패널 닫기
        purchasingItemUI.MarkAsSold();          // 구매한 아이템을 '판매 완료'로 표시
        UpdatePlayerGoldUI();                   // 골드 UI 업데이트

        Debug.Log($"구매 및 강화 완료: '{chosenBasicRune.SO.displayName}' -> '{runeToPurchase.displayName}'");
    }

    public void UpdatePlayerGoldUI()
    {
        // 1. 골드 텍스트 UI와 플레이어 인스턴스가 모두 존재하는지 확인합니다. (Null 오류 방지)
        if (playerGoldText != null && Player.Instance != null)
        {
            // 2. Player의 Gold(int) 값을 문자열(string)으로 변환하여 UI 텍스트에 할당합니다.
            playerGoldText.text = Player.Instance.Gold.ToString();

            // (선택) "100 G" 와 같이 단위를 붙이고 싶다면 아래처럼 사용할 수 있습니다.
            // playerGoldText.text = $"{Player.Instance.Gold} G";
        }
    }

}