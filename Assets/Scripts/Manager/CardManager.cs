using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using Unity.VisualScripting;
//using UnityEditor.Experimental.GraphView;
using UnityEngine;
using static UnityEngine.EventSystems.EventTrigger;
using Random = UnityEngine.Random;

public class CardManager : MonoBehaviour
{
    public static CardManager Inst { get; private set; } //싱글톤 - 최초 생성한 객체를 재사용함으로 일관성 보장
    void Awake() => Inst = this;

    [SerializeField] ItemSO itemSO;
    [SerializeField] GameObject cardPrefab;
    [SerializeField] List<Card> myCards;
    [SerializeField] Transform cardSpawnPoint;
    [SerializeField] Transform myCardLeft;
    [SerializeField] Transform myCardRight;
    [SerializeField] ECardState eCardState;
    //
    [SerializeField] Transform graveyardParent; // 묘지 위치
    [SerializeField] LayerMask enemyLayerMask; // 적 레이어 마스크
    private Card draggingCard;                // 현재 드래그 중인 카드

    //
    List<Item> itemBuffer;
    Card selectCard;
    bool isMyCardDrag; //카드 드래그
    bool onMyCardArea;


    // Player 참조
    [SerializeField] Player player; // 씬에 Player 오브젝트를 할당


    enum ECardState { Nothing, CanMouseOver, CanMouseDrag }

    public Item PopItem()
    {
        if (itemBuffer.Count == 0)
            SetupItemBuffer();

        Item item = itemBuffer[0]; //첫번째 카드 item 변수로 가져오고
        itemBuffer.RemoveAt(0);    //리스트에서 제거
        return item;
    }

    void SetupItemBuffer()
    {
        itemBuffer = new List<Item>(100); //100의 용량 잡아둠
        for (int i = 0; i < itemSO.items.Length; i++)
        {
            Item item = itemSO.items[i];
            for (int j = 0; j < item.percent; j++)
                itemBuffer.Add(item); //퍼센트만큼 배열 추가 (총100개 중 10%면 10개의 배열이 추가됨)
        }

        for (int i = 0; i < itemBuffer.Count; i++) //순서 섞기(셔플)
        {
            int rand = Random.Range(i, itemBuffer.Count);
            Item temp = itemBuffer[i];
            itemBuffer[i] = itemBuffer[rand];
            itemBuffer[rand] = temp;
        }
    }



    // Start is called before the first frame update
    void Start()
    {
        SetupItemBuffer();
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
        if (myTurn)
        {
            //코스트 모두 소모 시 CanMouseOver만 가능하게
        }
        SetECardState(); // 턴 시작 시 카드 상태 업데이트

    }



    // Update is called once per frame
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


            //if (draggingCard != null && draggingCard.CompareTag("attack")) // 공격 카드 드래그 시
            //{
            //    Vector3 mousePos = Utils.MousePos;
            //    draggingCard.MoveTransform(new PRS(mousePos, Utils.QI, draggingCard.originPRS.scale), false);

            //}

        }
    }
    public void SendAllCardsToGraveyard()
    {
        // myCards 리스트를 복사하여 순회 (리스트 수정 방지)
        List<Card> cardsToSend = new List<Card>(myCards);
        foreach (var card in cardsToSend)
        {
            Graveyard.Instance.GraveAddCard(card.gameObject);
            myCards.Remove(card);
            Destroy(card.gameObject); // 또는 비활성화: card.gameObject.SetActive(false);
        }
    }


    void DetectCardArea() //핸드(onMyCardArea)와 필드 레이어 구분 
    {
        RaycastHit2D[] hits = Physics2D.RaycastAll(Utils.MousePos, Vector3.forward); ////마우스에서 충돌한 RaycastHit를 가져옴
        int layer = LayerMask.NameToLayer("MyCardArea");
        onMyCardArea = Array.Exists(hits, x => x.collider.gameObject.layer == layer);
    }
    // 카드 목록을 묘지로 보내는 메서드++


    public void AddCard(bool isMine) //카드 드로우
    {
        var cardObject = Instantiate(cardPrefab, cardSpawnPoint.position, Utils.QI);
        var card = cardObject.GetComponent<Card>();
        card.Setup(PopItem(), isMine, player);
        myCards.Add(card); //턴 확인 

        SetOriginOrder(isMine);
        CardAlignment(isMine);
    }

    void SetOriginOrder(bool isMine) //order를 정렬
    {
        int count = myCards.Count;
        for (int i = 0; i < count; i++)
        {
            var targetCard = myCards[i];
            targetCard?.GetComponent<Order>().SetOriginOrder(i);
        }
    }
    void CardAlignment(bool isMine) //카드 정렬
    {
        List<PRS> originCardPRSs = new List<PRS>();
        if (isMine)
            originCardPRSs = RoundAlignment(myCardLeft, myCardRight, myCards.Count, 0.5f, Vector3.one * 0.6f);


        var targetCards = myCards;
        for (int i = 0; i < targetCards.Count; i++)
        {
            if (i < originCardPRSs.Count) // 인덱스 범위 확인++
            {
                var targetCard = targetCards[i];

                //targetCard.originPRS = new PRS(Vector3.zero, Utils.QI, Vector3.one * 1.9f);
                targetCard.originPRS = originCardPRSs[i];
                targetCard.MoveTransform(targetCard.originPRS, true, 0.7f);
            }
        }

    }
    List<PRS> RoundAlignment(Transform leftTr, Transform rightTr, int objCount, float height, Vector3 scale)
    {
        float[] objLerps = new float[objCount];
        List<PRS> results = new List<PRS>(objCount); //용량 잡아둠

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
            { //카드 4개 이후부터 커브 
                float curve = Mathf.Sqrt(Mathf.Pow(height, 2) - Mathf.Pow(objLerps[i] - 0.5f, 2));
                curve = height >= 0 ? curve : -curve;
                targetPos.y += curve;
                targetRot = Quaternion.Slerp(leftTr.rotation, rightTr.rotation, objLerps[i]);

            }
            results.Add(new PRS(targetPos, targetRot, scale));

        }
        return results;
    }
    public void CardMouseOver(Card card)
    {
        if (eCardState == ECardState.Nothing) return;
        selectCard = card; //마우스를 올린 카드
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
        draggingCard = selectCard; // 현재 선택된 카드를 드래그 중인 카드로 설정
    }
    public void CardMouseUp()
    {
        isMyCardDrag = false;

        if (eCardState != ECardState.CanMouseDrag || draggingCard == null)
            return;

        // 플레이어의 마나가 0이면 바로 카드 사용 취소 및 원위치 복귀
        if (player.CurrentMana <= 0)
        {
            draggingCard.MoveTransform(draggingCard.originPRS, true, 0.5f);
            Debug.Log("플레이어의 마나가 0이므로, 카드 사용이 불가능합니다. 카드가 손으로 돌아옴.");
            draggingCard = null;
            return;
        }

        Vector3 mousePos = Utils.MousePos;
        // 드롭 위치에 적 오브젝트가 있는지 확인
        Collider2D hit = Physics2D.OverlapPoint(mousePos, enemyLayerMask);
        bool isOverEnemy = false;
        Enemy enemy = null;

        if (hit != null)
        {
            enemy = hit.GetComponent<Enemy>();
            if (enemy != null)
            {
                isOverEnemy = true;
            }
        }


        // ===========================
        // [1] 적 위에 드롭된 경우
        // ===========================
        if (isOverEnemy)
        {
            player.AtkAni();

            // 공격 카드
            if (draggingCard.item.type == ItemType.Attack)
            {
                draggingCard.AttackPlayEffect(enemy);
                GoToGrave();
                Debug.Log("공격카드가 적에게 사용됨");
            }
            //// 스킬 카드
            //else if (draggingCard.item.type == ItemType.Skill)
            //{
            //    // ex) 스킬이 적에게도 적용되는 스킬이라면 여기에 로직 작성
            //    // 아니면 스킬이 적 타겟을 필요로 하지 않으면 무효
            //    draggingCard.SkillPlayEffect(enemy);
            //    GoToGrave();
            //    Debug.Log("스킬카드(적 대상) 사용");
            //}
            // 방어 카드 -> 적에게 사용 불가능
            else if (draggingCard.item.type == ItemType.Skill)
            {
                // 방어카드는 적 위에 놓으면 무효 -> 손으로 돌아옴
                draggingCard.MoveTransform(draggingCard.originPRS, true, 0.5f);
                Debug.Log("스킬카드는 적에게 사용할 수 없습니다. (손으로 돌아옴)");
            }
            //// 저주 카드
            //else if (draggingCard.item.type == ItemType.Curse)
            //{
            //    // ex) 저주 카드를 적에게 쓸 수도 있고, 아니면 무효화할 수도 있음
            //    draggingCard.CursePlayEffect(enemy);
            //    GoToGrave();
            //    Debug.Log("저주카드 사용");
            //}
            else
            {
                // 기타 타입
                draggingCard.MoveTransform(draggingCard.originPRS, true, 0.5f);
            }
        }
        else
        {
            // ===========================
            // [2] 적이 아닌 곳(= 필드)에 드롭
            // ===========================
            // 방어카드는 이 위치에서 발동
            if (draggingCard.item.type == ItemType.Skill)
            {
                draggingCard.SkillPlayEffect();
                GoToGrave();
                Debug.Log("스킬카드 필드 사용");
            }
            // 공격/스킬/저주 카드는 필드에 사용 불가능 -> 손으로 복귀
            else
            {
                draggingCard.MoveTransform(draggingCard.originPRS, true, 0.5f);
                Debug.Log("이 카드는 적에게만 사용할 수 있습니다.");
            }
        }

        draggingCard = null;
    }



    void GoToGrave() //카드 묘지로 보내는 함수
    {
        // 카드 묘지로 이동
        Graveyard.Instance.GraveAddCard(draggingCard.gameObject);
        // 카드 리스트에서 제거
        myCards.Remove(draggingCard);

        // 카드 GameObject 파괴
        Destroy(draggingCard.gameObject);

        // 카드 정렬 업데이트
        CardAlignment(true);
    }


    void EnlargeCard(bool isEnlage, Card card) //카드 확대 및 축소
    {
        if (isEnlage)
        {
            Vector3 enlargePos = new Vector3(card.originPRS.pos.x, 0f, -10f);

            card.MoveTransform(new PRS(enlargePos, Utils.QI, Vector3.one * 2.5f), false);
        }
        else
            card.MoveTransform(card.originPRS, false);

        card.GetComponent<Order>().SetMostFrontOrder(isEnlage);

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


}