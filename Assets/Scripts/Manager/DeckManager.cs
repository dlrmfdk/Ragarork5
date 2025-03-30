//using System.Collections;
//using System.Collections.Generic;
//using UnityEngine;

//public class DeckManager : MonoBehaviour
//{
//    // 싱글톤 인스턴스
//    public static DeckManager Instance { get; private set; }

//    // 카드 데이터를 보관하는 ScriptableObject (ItemSO)
//    [SerializeField] private ItemSO itemSO;

//    // 실제 덱 (Deck 클래스)
//    private Deck deck;

//    void Awake()
//    {
//        if (Instance == null)
//            Instance = this;
//        else
//            Destroy(gameObject);
//    }

//    void Start()
//    {
//        BuildDeck();
//        deck.Shuffle();
//    }

//    /// <summary>
//    /// CSV 데이터(ItemSO)에 있는 모든 카드 정보를 기반으로 덱을 구성합니다.
//    /// 각 카드의 등장 확률(percent)에 따라 여러 장 추가합니다.
//    /// </summary>
//    public void BuildDeck()
//    {
//        deck = new Deck();
//        if (itemSO == null || itemSO.items == null)
//        {
//            Debug.LogError("DeckManager: ItemSO가 할당되지 않았거나, 아이템이 없습니다.");
//            return;
//        }

//        // ItemSO의 각 카드를, percent 만큼 덱에 추가
//        foreach (Item item in itemSO.items)
//        {
//            // percent가 카드의 등장 횟수를 나타낸다고 가정 (예: 10이면 10장 추가)
//            int count = Mathf.RoundToInt(item.percent);
//            deck.AddCardMultiple(item, count);
//        }
//    }

//    /// <summary>
//    /// 덱에서 카드 한 장을 뽑습니다.
//    /// 덱이 비었으면 null을 반환합니다.
//    /// </summary>
//    public Item DrawCard()
//    {
//        if (deck.Count == 0)
//        {
//            Debug.Log("DeckManager: 덱이 비었습니다.");
//            return null;
//        }
//        return deck.DrawCard();
//    }

//    /// <summary>
//    /// 덱에 카드 한 장을 추가합니다.
//    /// </summary>
//    public void AddCardToDeck(Item item)
//    {
//        deck.AddCard(item);
//    }

//    /// <summary>
//    /// 덱에서 특정 카드를 제거합니다.
//    /// </summary>
//    public bool RemoveCardFromDeck(Item item)
//    {
//        return deck.RemoveCardFromDeck(item);
//    }

//    /// <summary>
//    /// 현재 덱에 남은 카드 수를 반환합니다.
//    /// </summary>
//    public int GetDeckCount()
//    {
//        return deck.Count;
//    }

//    /// <summary>
//    /// 덱의 모든 카드 목록을 반환합니다.
//    /// </summary>
//    public List<Item> GetDeckList()
//    {
//        return deck.GetCards();
//    }
//}
