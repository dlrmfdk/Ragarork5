using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.VisualScripting;
using static UnityEngine.EventSystems.EventTrigger;
using Random = UnityEngine.Random;

public class CardManager : MonoBehaviour
{
    public static CardManager Inst { get; private set; } // 싱글톤: 최초 생성된 객체를 재사용
    void Awake() => Inst = this;

    [SerializeField] ItemSO itemSO;
    [SerializeField] GameObject cardPrefab;
    [SerializeField] List<Card> myCards;
    [SerializeField] Transform cardSpawnPoint;
    [SerializeField] Transform myCardLeft;
    [SerializeField] Transform myCardRight;
    [SerializeField] ECardState eCardState;

    [SerializeField] Transform graveyardParent; // 묘지 위치 (비주얼용)
    [SerializeField] LayerMask enemyLayerMask;    // 적 레이어 마스크
    private Card draggingCard;                     // 현재 드래그 중인 카드

    // 덱 관리용 리스트: deck은 현재 덱, discardPile은 사용한 카드들의 아이템을 저장
    List<Item> deck;
    List<Item> discardPile;

    Card selectCard;
    bool isMyCardDrag; // 카드 드래그 상태
    bool onMyCardArea;

    // Player 참조
    [SerializeField] Player player; // 씬에 Player 오브젝트 할당

    enum ECardState { Nothing, CanMouseOver, CanMouseDrag }

    #region 덱/묘지 시스템 구현

    // 덱 초기화: 초기 덱은 "강타" 5장, "방어하기" 5장으로 구성 (ItemSO는 전체 카드 정보 읽기 전용)
    void SetupDeck()
    {
        deck = new List<Item>(100);
        discardPile = new List<Item>(); // discardPile 초기화

        // ItemSO의 모든 카드 중에서 이름이 "강타"와 "방어하기"인 카드만 추가
        for (int i = 0; i < itemSO.items.Length; i++)
        {
            Item item = itemSO.items[i];
            if (item.name == "강타")
            {
                for (int j = 0; j < 5; j++)
                    deck.Add(item);
            }
            else if (item.name == "방어하기")
            {
                for (int j = 0; j < 5; j++)
                    deck.Add(item);
            }
        }
        ShuffleDeck(deck);
    }

    // Fisher-Yates 알고리즘을 사용한 셔플
    void ShuffleDeck(List<Item> list)
    {
        for (int i = 0; i < list.Count; i++)
        {
            int rand = Random.Range(i, list.Count);
            Item temp = list[i];
            list[i] = list[rand];
            list[rand] = temp;
        }
    }

    // 덱에서 카드 뽑기: 덱이 비어있으면 discardPile에서 카드 가져오기
    public Item PopItem()
    {
        if (deck == null || deck.Count == 0)
        {
            if (discardPile != null && discardPile.Count > 0)
            {
                deck.AddRange(discardPile);
                discardPile.Clear();
                ShuffleDeck(deck);
                Debug.Log("묘지에 저장된 카드를 덱으로 옮기고 셔플했습니다.");
            }
            else
            {
                // 항상 덱 또는 묘지에 카드가 있다고 가정하므로, 여기서 아무것도 하지 않음
                Debug.Log("뽑을 카드가 없습니다!"); // 또는 예외 처리
                return null;
            }
        }
        Item item = deck[0];
        deck.RemoveAt(0);
        return item;
    }


    // 카드 사용 후 해당 카드를 discardPile에 추가
    void AddToDiscardPile(Item item)
    {
        discardPile.Add(item);
    }

    // 보상 카드 추가: 보상으로 획득한 카드를 플레이어 덱에 추가
    public void AddRewardCardToDeck(Item rewardCard)
    {
        if (rewardCard == null)
        {
            Debug.LogWarning("추가할 보상 카드가 null입니다.");
            return;
        }
        deck.Add(rewardCard);
        ShuffleDeck(deck);
        Debug.Log($"{rewardCard.name} 카드가 덱에 추가되었습니다.");
    }

    #endregion

    #region Unity 이벤트 및 카드 생성/정렬

    void Start()
    {
        SetupDeck();
        TurnManager.OnAddCard += AddCard;
        TurnManager.OnTurnStarted += OnTurnStarted;
    }
    void OnDestroy()
    {
        TurnManager.OnAddCard -= AddCard;
        TurnManager.OnTurnStarted -= OnTurnStarted;
    }
    void OnTurnStarted(bool myTurn)
    {
        // 턴 시작 시 카드 상태 업데이트 등 추가 로직 구현 가능
        SetECardState();
    }

    // 매 프레임 카드 드래그 및 마우스 영역 체크
    void Update()
    {
        if (isMyCardDrag)
        {
            CardDrag();
        }
        DetectCardArea();
    }

    void CardDrag()
    {
        if (!onMyCardArea && draggingCard != null)
        {
            Vector3 mousePos = Utils.MousePos;
            draggingCard.MoveTransform(new PRS(mousePos, Utils.QI, draggingCard.originPRS.scale), false);
        }
    }

    // 마우스 포인터가 카드 영역(MyCardArea)에 있는지 감지
    void DetectCardArea()
    {
        RaycastHit2D[] hits = Physics2D.RaycastAll(Utils.MousePos, Vector3.forward);
        int layer = LayerMask.NameToLayer("MyCardArea");
        onMyCardArea = Array.Exists(hits, x => x.collider.gameObject.layer == layer);
    }

    // 덱에서 카드를 뽑아 인스턴스화하여 손(Hand)에 추가
    public void AddCard(bool isMine)
    {
        Item drawnItem = PopItem();
        if (drawnItem == null) return; // 뽑을 카드가 없으면 리턴

        var cardObject = Instantiate(cardPrefab, cardSpawnPoint.position, Utils.QI);
        var card = cardObject.GetComponent<Card>();
        card.Setup(drawnItem, isMine, player);
        myCards.Add(card);

        SetOriginOrder(isMine);
        CardAlignment(isMine);
    }

    void SetOriginOrder(bool isMine)
    {
        int count = myCards.Count;
        for (int i = 0; i < count; i++)
        {
            var targetCard = myCards[i];
            targetCard?.GetComponent<Order>().SetOriginOrder(i);
        }
    }
    void CardAlignment(bool isMine)
    {
        List<PRS> originCardPRSs = new List<PRS>();
        if (isMine)
            originCardPRSs = RoundAlignment(myCardLeft, myCardRight, myCards.Count, 0.5f, Vector3.one * 0.6f);

        for (int i = 0; i < myCards.Count; i++)
        {
            if (i < originCardPRSs.Count)
            {
                var targetCard = myCards[i];
                targetCard.originPRS = originCardPRSs[i];
                targetCard.MoveTransform(targetCard.originPRS, true, 0.7f);
            }
        }
    }
    List<PRS> RoundAlignment(Transform leftTr, Transform rightTr, int objCount, float height, Vector3 scale)
    {
        float[] objLerps = new float[objCount];
        List<PRS> results = new List<PRS>(objCount);

        switch (objCount)
        {
            case 1: objLerps = new float[] { 0.5f }; break;
            case 2: objLerps = new float[] { 0.27f, 0.73f }; break;
            case 3: objLerps = new float[] { 0.1f, 0.5f, 0.9f }; break;
            default:
                float interval = 1f / (objCount - 1);
                for (int i = 0; i < objCount; i++)
                    objLerps[i] = interval * i;
                break;
        }
        for (int i = 0; i < objCount; i++)
        {
            var targetPos = Vector3.Lerp(leftTr.position, rightTr.position, objLerps[i]);
            var targetRot = Utils.QI;
            if (objCount >= 4)
            {
                float curve = Mathf.Sqrt(Mathf.Pow(height, 2) - Mathf.Pow(objLerps[i] - 0.5f, 2));
                curve = height >= 0 ? curve : -curve;
                targetPos.y += curve;
                targetRot = Quaternion.Slerp(leftTr.rotation, rightTr.rotation, objLerps[i]);
            }
            results.Add(new PRS(targetPos, targetRot, scale));
        }
        return results;
    }

    #endregion

    #region 카드 마우스 인터랙션

    public void CardMouseOver(Card card)
    {
        if (eCardState == ECardState.Nothing) return;
        selectCard = card;
        EnlargeCard(true, card);
    }
    public void CardMouseExit(Card card)
    {
        EnlargeCard(false, card);
    }
    public void CardMouseDown()
    {
        if (eCardState != ECardState.CanMouseDrag) return;
        isMyCardDrag = true;
        draggingCard = selectCard;
    }
    public void CardMouseUp()
    {
        isMyCardDrag = false;
        if (eCardState != ECardState.CanMouseDrag || draggingCard == null)
            return;

        if (player.CurrentMana <= 0)
        {
            draggingCard.MoveTransform(draggingCard.originPRS, true, 0.5f);
            Debug.Log("플레이어의 마나가 0이므로, 카드 사용이 불가능합니다. 카드가 손으로 돌아옴.");
            draggingCard = null;
            return;
        }

        Vector3 mousePos = Utils.MousePos;
        Collider2D hit = Physics2D.OverlapPoint(mousePos, enemyLayerMask);
        bool isOverEnemy = false;
        Enemy enemy = null;

        if (hit != null)
        {
            enemy = hit.GetComponent<Enemy>();
            if (enemy != null)
                isOverEnemy = true;
        }

        if (isOverEnemy)
        {
            player.AtkAni();
            if (draggingCard.item.type == ItemType.Attack)
            {
                draggingCard.AttackPlayEffect(enemy);
                GoToGrave();
                Debug.Log("공격카드가 적에게 사용됨");
            }
            else if (draggingCard.item.type == ItemType.Skill)
            {
                draggingCard.MoveTransform(draggingCard.originPRS, true, 0.5f);
                Debug.Log("스킬카드는 적에게 사용할 수 없습니다. (손으로 돌아옴)");
            }
            else
            {
                draggingCard.MoveTransform(draggingCard.originPRS, true, 0.5f);
            }
        }
        else
        {
            if (draggingCard.item.type == ItemType.Skill)
            {
                draggingCard.SkillPlayEffect();
                GoToGrave();
                Debug.Log("스킬카드 필드 사용");
            }
            else
            {
                draggingCard.MoveTransform(draggingCard.originPRS, true, 0.5f);
                Debug.Log("이 카드는 적에게만 사용할 수 있습니다.");
            }
        }
        draggingCard = null;
    }

    // 카드 사용 후, 카드의 아이템을 discardPile에 추가한 후 카드 GameObject를 제거
    void GoToGrave()
    {
        AddToDiscardPile(draggingCard.item);
        Graveyard.Instance.GraveAddCard(draggingCard.gameObject);
        myCards.Remove(draggingCard);
        Destroy(draggingCard.gameObject);
        CardAlignment(true);
    }

    void EnlargeCard(bool isEnlarge, Card card)
    {
        if (isEnlarge)
        {
            Vector3 enlargePos = new Vector3(card.originPRS.pos.x, 0f, -10f);
            card.MoveTransform(new PRS(enlargePos, Utils.QI, Vector3.one * 2.5f), false);
        }
        else
        {
            card.MoveTransform(card.originPRS, false);
        }
        card.GetComponent<Order>().SetMostFrontOrder(isEnlarge);
    }
    public void SetECardState()
    {
        if (TurnManager.Inst.isLoading)
            eCardState = ECardState.Nothing;
        else if (!TurnManager.Inst.myTurn)
            eCardState = ECardState.CanMouseOver;
        else if (TurnManager.Inst.myTurn)
            eCardState = ECardState.CanMouseDrag;
    }

    // (추후 소멸 카드 등 특수 카드 처리 로직 추가 가능)
    #endregion

    // 모든 카드를 묘지(Discard)로 보내는 함수
    public void SendAllCardsToGraveyard()
    {
        List<Card> cardsToSend = new List<Card>(myCards);
        foreach (var card in cardsToSend)
        {
            AddToDiscardPile(card.item);
            Graveyard.Instance.GraveAddCard(card.gameObject);
            myCards.Remove(card);
            Destroy(card.gameObject);
        }
    }
}