//using UnityEngine;



//public class CurseCardEffect : BaseCardEffect
//{
//    private CurseType curseType;

//    public void Initialize(CurseType type)
//    {
//        curseType = type;
//    }

//    public override void Execute(Player player, Enemy target = null)
//    {

//// cost 열이 마나 코스트이므로, 플레이어가 이 코스트만큼의 마나를 가지고 있는지 확인하고 사용합니다.
//if (!player.TryUseMana(cardData.cost))
//{
//    Debug.Log("마나가 부족하여 카드를 사용할 수 없습니다.");
//    return;
//}
//        switch (curseType)
//        {
//            case CurseType.Burn:
//                player.ApplyBurn(5);
//                break;
//            case CurseType.ButterflyCurse:
//                Debug.Log("나비의 저주: 효과없음 (소멸됨)");
//                break;
//        }
//    }
//}
