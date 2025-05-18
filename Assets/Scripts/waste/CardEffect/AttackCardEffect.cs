//using System.Collections;
//using System.Collections.Generic;
//using UnityEngine;



//public class AttackCardEffect : BaseCardEffect
//{
//    private AttackType attackType;
//    private int power;

//    private Dictionary<AttackType, string> attackNameMap = new Dictionary<AttackType, string>()
//    {
//        { AttackType.Smash, "강타" },
//        { AttackType.MultiStrike, "연속 타격" },
//        { AttackType.Stronger, "더 강하게!" },
//        { AttackType.CostOfStrike, "타격의 대가" },
//        { AttackType.PoisonFang, "독 송곳니" },
//        { AttackType.FenrirFang, "펜리르의 송곳니" },
//        { AttackType.JotunFist, "요툰의 주먹" },
//        { AttackType.FireStrike, "불꽃의 일격" },
//        { AttackType.BloodDrain, "흡혈" },
//    };

//    public void Initialize(AttackType type, int addedPower = 0)
//    {
//        attackType = type;
//        power = addedPower;
//    }

//    public override void Execute(Player player, Enemy target = null)
//    {
//        if (target == null)
//        {
//            Debug.LogWarning("AttackCardEffect: 공격 대상이 없습니다!");
//            return;
//        }

//        string cardName = attackNameMap[attackType];
//        CardData cardData = CardDatabase.Instance.GetCardDataByName(cardName);
//        if (cardData == null)
//        {
//            Debug.LogWarning($"CSV에서 '{cardName}' 카드를 찾을 수 없습니다!");
//            return;
//        }

//        if (!player.TryUseMana(cardData.cost))
//        {
//            Debug.Log("마나가 부족하여 카드를 사용할 수 없습니다.");
//            return;
//        }

//        int damage = (int)cardData.defaultValue + power;

//        switch (attackType)
//        {
//            case AttackType.MultiStrike:
//                damage *= 3;
//                break;
//            //case AttackType.CostOfStrike:
//            //    damage = player.CountAttackCardsInDeck();
//            //    break;
//            case AttackType.FenrirFang:
//                if (target.GetCurrentHpPercentage() <= 0.5f) damage *= 2;
//                break;
//            case AttackType.PoisonFang:
//                target.ApplyPoison(3);
//                break;
//            //case AttackType.FireStrike:
//            //    player.AddCardToDeck("화상");
//            //    break;
//            case AttackType.BloodDrain:
//                player.Heal(damage);
//                break;
//        }

//        target.Hit(damage, player);
//        Debug.Log($"{cardData.cardName}: {damage} 피해 → {target.EnemyData.EnemyName}");
//    }
//}
