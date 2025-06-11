// ShopManager.cs (새로 만들기 - Legend 희귀도에 맞게 수정된 버전)
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

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

    private void Awake()
    {
        if (Instance == null) { Instance = this; }
        else { Destroy(gameObject); }
    }

    private void Start()
    {
        GenerateShopItems();
        UpdatePlayerGoldUI();
        // exitButton 리스너 설정 (예: 맵 씬으로 돌아가기)
        // exitButton.onClick.AddListener(() => UnityEngine.SceneManagement.SceneManager.LoadScene("MapScene"));
    }

    private void GenerateShopItems()
    {
        foreach (Transform child in itemContainer)
        {
            Destroy(child.gameObject);
        }

        var availableRunes = new List<RuneSO>(allSellableRunes);
        var runesToDisplay = new List<RuneSO>();
        int count = Mathf.Min(numberOfItemsToShow, availableRunes.Count);

        for (int i = 0; i < count; i++)
        {
            int randomIndex = Random.Range(0, availableRunes.Count);
            runesToDisplay.Add(availableRunes[randomIndex]);
            availableRunes.RemoveAt(randomIndex);
        }

        foreach (var rune in runesToDisplay)
        {
            GameObject itemGO = Instantiate(shopItemPrefab, itemContainer);
            itemGO.GetComponent<ShopItemUI>().Setup(rune);
        }
    }

    // 희귀도에 따른 가격을 반환합니다. (Legend에 맞게 수정됨)
    public int GetPriceForRarity(RuneRarity rarity)
    {
        switch (rarity)
        {
            case RuneRarity.Common: return 100;
            case RuneRarity.Rare: return 150;
            case RuneRarity.Legend: return 200; // Epic -> Legend 로 수정
            default: return 999;
        }
    }

    public bool AttemptPurchase(RuneSO rune, int price)
    {
        if (Player.Instance.Gold < price)
        {
            Debug.Log("골드가 부족합니다!");
            return false;
        }

        Player.Instance.AddGold(-price);
        RuneDeckManager.Instance.AddRuneToDeck(rune.displayName); // ID 대신 displayName을 사용하거나, RuneSO에 runeID 필드를 추가하여 사용하세요.

        Debug.Log($"{rune.displayName} 구매 완료!");
        UpdatePlayerGoldUI();
        return true;
    }

    public void UpdatePlayerGoldUI()
    {
        if (playerGoldText != null && Player.Instance != null)
        {
            playerGoldText.text = $"소지 골드: {Player.Instance.Gold}";
        }
    }
}