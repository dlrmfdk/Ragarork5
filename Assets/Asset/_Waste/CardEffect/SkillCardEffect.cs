//using System.Collections;
//using System.Collections.Generic;
//using UnityEngine;

//public class SkillCardEffect : BaseCardEffect
//{
//    private SkillType skillType;
//    private int power;
//    private Card card;

//    // SkillType에 대응하는 CSV 카드 이름 매핑
//    private Dictionary<SkillType, string> skillNameMap = new Dictionary<SkillType, string>()
//    {
//        { SkillType.Defend, "방어하기" },
//        { SkillType.HeavenlyEnergy,"선천진기" },
//        { SkillType.PoisonMist, "독안개" },
//        { SkillType.JormungandAcid, "요르문간드의 산성액" },
//        { SkillType.HeimdallPath, "헤임달의 통로" },       
//        { SkillType.ValhallaShield, "발할라의 방패" },
//        { SkillType.HellfireSummon, "지옥불 소환" },
//        { SkillType.SurturFury, "수르트의 분노" },
//        { SkillType.LokiTrick, "로키의 장난" }
//    };

//    /// <summary>HeavenlyEnergy
//    /// 효과 초기화: 스킬 타입과 추가 파워 설정
//    /// </summary>
//    public void Initialize(SkillType type, int powerVal = 0, Card cardRef = null)
//    {
//        skillType = type;
//        power = powerVal;
//        card = cardRef;
//    }

//    /// <summary>
//    /// 스킬 카드 효과 실행
//    /// </summary>
//    public override void Execute(Player player, Enemy target = null)
//    {
//        string cardName = skillNameMap[skillType];
//        CardData cardData = CardDatabase.Instance.GetCardDataByName(cardName);

//        // cost 열이 마나 코스트이므로, 플레이어가 이 코스트만큼의 마나를 가지고 있는지 확인하고 사용합니다.
//        if (!player.TryUseMana(cardData.cost))
//        {
//            Debug.Log("마나가 부족하여 카드를 사용할 수 없습니다.");
//            return;
//        }

//        if (cardData == null)
//        {
//            Debug.LogWarning($"CSV에서 '{cardName}' 스킬 카드를 찾을 수 없습니다!");
//            return;
//        }

//        switch (skillType)
//        {
//            case SkillType.Defend:
//                // 방어하기: 플레이어의 방어도를 올린다.
//                player.IncreaseDefense((int)cardData.defaultValue + power);
//                break;
//            //case SkillType.HeavenlyEnergy:
//            //    // 선천진기: 이번 턴에 주는 데미지가 2배가 된다.
//            //    //player.DoubleDamageThisTurn();
//            //    break;
//            case SkillType.PoisonMist:
//                ///독안개 카드를 쓸 때 Card.cs에 있는 atksound[0]을 재생
//                if (card != null)
//                {
//                    card.PlaySound(card.GetAttackSound(0)); // 사운드 재생
//                }

//                // 독안개: 모든 적에게 독을 부여 (예시)
//                EnemySpawner.Instance.ApplyPoisonToAllEnemies((int)cardData.defaultValue);
//                break;
//            case SkillType.JormungandAcid:
//                // 요르문간드의 산성액: 모든 적의 독을 2배 증가시키고 즉시 데미지 적용 (예시)
//                EnemySpawner.Instance.BoostPoisonOnAllEnemiesAndDamage((int)cardData.defaultValue);
//                break;
//            case SkillType.HeimdallPath:
//                // 헤임달의 통로: 카드를 1장 뽑는다.
//                player.DrawCards(1);
//                break;
//            case SkillType.ValhallaShield:
//                // 발할라의 방패: 1턴 동안 적의 공격 피해를 받지 않는다.
//                player.SetInvincibleTurn(1);
//                break;
//            //case SkillType.HellfireSummon:
//            //    // 지옥불 소환: 덱과 패에 화상 카드를 2장씩 추가하고, 적에게 데미지 적용
//            //    player.AddCardToDeck("화상");
//            //    target?.Hit((int)cardData.defaultValue + power, player);
//            //    break;
//            //case SkillType.SurturFury:
//            //    // 수르트의 분노: 덱과 패의 화상 카드 수 * 5 데미지를 주고 화상 카드를 소멸시킴
//            //    EnemySpawner.Instance.ExplodeBurnCards((int)cardData.defaultValue);
//            //    break;
//            case SkillType.LokiTrick:
//                // 로키의 장난: 적에게 저주와 약화 상태를 2턴 동안 부여
//                target?.ApplyDebuff("저주", 2);
//                target?.ApplyDebuff("약화", 2);
//                break;
//            default:
//                Debug.LogWarning($"SkillCardEffect: {cardData.cardName} 효과가 구현되지 않았습니다.");
//                break;
//        }

//        Debug.Log($"{cardData.cardName}: 효과 발동!");
//    }
//}
