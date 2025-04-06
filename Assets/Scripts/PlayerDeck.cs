//using System.Collections.Generic;
//using UnityEngine;

//public class PlayerDeck
//{
//    public List<Item> deck = new List<Item>();

//    /// <summary>
//    /// 초기 덱 구성: ItemSO에서 "강타"와 "방어하기" 카드 5장씩 추가합니다.
//    /// </summary>
//    public void InitializeInitialDeck(ItemSO itemSO)
//    {
//        deck.Clear();

//        // ItemSO에 저장된 모든 카드 중 "강타"와 "방어하기"를 찾아 5장씩 추가
//        foreach (Item card in itemSO.items)
//        {
//            if (card.name == "강타")
//            {
//                for (int i = 0; i < 5; i++)
//                    deck.Add(card);
//            }
//            else if (card.name == "방어하기")
//            {
//                for (int i = 0; i < 5; i++)
//                    deck.Add(card);
//            }
//        }

//        ShuffleDeck();
//    }

//    /// <summary>
//    /// Fisher-Yates 알고리즘을 사용하여 덱을 셔플합니다.
//    /// </summary>
//    public void ShuffleDeck()
//    {
//        for (int i = 0; i < deck.Count; i++)
//        {
//            int rand = Random.Range(i, deck.Count);
//            Item temp = deck[i];
//            deck[i] = deck[rand];
//            deck[rand] = temp;
//        }
//    }

//    /// <summary>
//    /// 덱에서 카드를 뽑습니다.
//    /// </summary>
//    public Item DrawCard()
//    {
//        if (deck.Count == 0)
//        {
//            Debug.Log("PlayerDeck: 덱이 비었습니다.");
//            return null;
//        }
//        Item card = deck[0];
//        deck.RemoveAt(0);
//        return card;
//    }

//    /// <summary>
//    /// 보상 등으로 추가된 카드를 덱에 추가합니다.
//    /// </summary>
//    public void AddCard(Item card)
//    {
//        if (card != null)
//        {
//            deck.Add(card);
//            ShuffleDeck();
//        }
//    }
//}
