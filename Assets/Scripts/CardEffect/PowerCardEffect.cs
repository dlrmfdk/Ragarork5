using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PowerCardEffect : BaseCardEffect
{
    private PowerType powerType;
    private int powerValue;

    // PowerType에 대응하는 CSV 카드 이름 매핑
    private Dictionary<PowerType, string> powerNameMap = new Dictionary<PowerType, string>()
    {
        { PowerType.BlessingOfStrength, "힘의 축복" },
        { PowerType.OdinWisdom, "오딘의 지혜" }
    };

    /// <summary>
    /// 효과 초기화: 파워 타입과 추가 값 설정
    /// </summary>
    public void Initialize(PowerType type, int addedValue = 0)
    {
        powerType = type;
        powerValue = addedValue;
    }

    /// <summary>
    /// 파워 카드 효과 실행
    /// </summary>
    public override void Execute(Player player, Enemy target = null)
    {
        string cardName = powerNameMap[powerType];
        CardData cardData = CardDatabase.Instance.GetCardDataByName(cardName);

        // cost 열이 마나 코스트이므로, 플레이어가 이 코스트만큼의 마나를 가지고 있는지 확인하고 사용합니다.
        if (!player.TryUseMana(cardData.cost))
        {
            Debug.Log("마나가 부족하여 카드를 사용할 수 없습니다.");
            return;
        }

        if (cardData == null)
        {
            Debug.LogWarning($"CSV에서 '{cardName}' 파워 카드를 찾을 수 없습니다!");
            return;
        }

        switch (powerType)
        {
            case PowerType.BlessingOfStrength:
                // 힘의 축복: 플레이어의 공격력 증가 (영구 버프)
                player.IncreaseAttack((int)cardData.defaultValue + powerValue);
                break;
            case PowerType.OdinWisdom:
                // 오딘의 지혜: 최대 마나량 1 증가
                player.IncreaseMaxMana(1);
                break;
            default:
                Debug.LogWarning($"PowerCardEffect: {cardData.cardName} 효과가 구현되지 않았습니다.");
                break;
        }

        Debug.Log($"{cardData.cardName}: 효과 발동!");
    }
}
