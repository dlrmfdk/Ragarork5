//using System.Collections;
//using System.Collections.Generic;
//using UnityEngine;

//[System.Serializable]
//public class Deck
//{
//    // 덱에 들어있는 카드 목록
//    [SerializeField] private List<Item> cards = new List<Item>();

//    // 덱에 있는 카드 수
//    public int Count => cards.Count;

//    // 카드를 덱에 추가합니다.
//    public void AddCard(Item card)
//    {
//        cards.Add(card);
//    }

//    // 특정 카드를 덱에 여러 장 추가합니다.
//    public void AddCardMultiple(Item card, int count)
//    {
//        for (int i = 0; i < count; i++)
//        {
//            cards.Add(card);
//        }
//    }

//    // 덱에서 맨 위의 카드를 뽑고, 목록에서 제거합니다.
//    public Item DrawCard()
//    {
//        if (cards.Count == 0)
//            return null;

//        Item card = cards[0];
//        cards.RemoveAt(0);
//        return card;
//    }

//    // 덱을 셔플합니다. (Fisher-Yates 알고리즘)
//    public void Shuffle()
//    {
//        for (int i = 0; i < cards.Count; i++)
//        {
//            int rand = Random.Range(i, cards.Count);
//            Item temp = cards[i];
//            cards[i] = cards[rand];
//            cards[rand] = temp;
//        }
//    }

//    // 덱의 모든 카드를 제거합니다.
//    public void Clear()
//    {
//        cards.Clear();
//    }

//    // 덱에 있는 카드 목록의 복사본을 반환합니다.
//    public List<Item> GetCards()
//    {
//        return new List<Item>(cards);
//    }

//    // 덱에서 특정 카드를 제거하는 메서드
//    public bool RemoveCardFromDeck(Item item)
//    {
//        return cards.Remove(item);
//    }
//}
