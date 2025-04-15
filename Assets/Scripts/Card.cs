using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
//using UnityEditor.Build; //TMP_Text 불러오는용
using DG.Tweening; //do TWeen

public class Card : MonoBehaviour
{
    [SerializeField] SpriteRenderer card; //카드
    [SerializeField] SpriteRenderer character; 
    [SerializeField] TMP_Text nameTMP; //카드 이름
    [SerializeField] TMP_Text attackTMP; //카드 공격력
    [SerializeField] TMP_Text healthTMP; //체력
    [SerializeField] Sprite cardFront; 
    //[SerializeField] Sprite cardBack; 

    public Item item;
    public CardData cardData;
    bool isFront; //앞뒷면 판단
    public PRS originPRS; //카드 위치
    private BaseCardEffect CardEffect; // 카드 효과 참조                                    
    private Enemy targetEnemy;
    private Player player;

    public void Setup(Item item, bool isFront, Player currentPlayer)
    {
        this.item = item;
        this.isFront = isFront;
        this.player = currentPlayer;
        

        if (this.isFront) //앞면
        {
            character.sprite = this.item.sprite;
            nameTMP.text = this.item.name;
            attackTMP.text = this.cardData.defaultValue.ToString(); //CSV에 있는 공격력
            //healthTMP.text = this.item.attack.ToString(); // 필요 시 체력도 설정

        }
        // 카드 타입에 따라 효과 컴포넌트 추가
        if (item.type == ItemType.Attack)
        {
            CardEffect = gameObject.AddComponent<AttackCardEffect>();
            ((AttackCardEffect)CardEffect).Initialize(item.attackType);
        }
        // 방어 카드 등 다른 타입도 추가 가능
        else if(item.type == ItemType.Skill)
        {
            CardEffect = gameObject.AddComponent<SkillCardEffect>();
            ((SkillCardEffect)CardEffect).Initialize(item.skillType);
        }

    }
    // 카드의 효과를 실행하는 메서드
    public void AttackPlayEffect(Enemy target)
    {
        this.targetEnemy = target;
        CardEffect?.Execute(player, target);
    }
    public void SkillPlayEffect()
    {
        CardEffect?.Execute(player);
    }

    private void OnMouseOver() 
    {
        if(isFront)
            CardManager.Inst.CardMouseOver(this);
    }
    private void OnMouseExit()
    {
        if(isFront)
            CardManager.Inst.CardMouseExit(this);
    }
    private void OnMouseDown()
    {
        if (isFront)
            CardManager.Inst.CardMouseDown();
    }
    private void OnMouseUp()
    {
        if (isFront)
            CardManager.Inst.CardMouseUp();
    }

    public void MoveTransform(PRS prs, bool useDotween, float dotweenTime = 0) //dotween-부드럽게 애니매이션 적용
    {
        if(useDotween)
        {
            transform.DOMove(prs.pos, dotweenTime);
            transform.DORotateQuaternion(prs.rot,dotweenTime); 
            transform.DOScale(prs.scale, dotweenTime);
        }
        else
        {
            transform.position = prs.pos;
            transform.rotation = prs.rot;
            transform.localScale = prs.scale;

        }
    }

}
