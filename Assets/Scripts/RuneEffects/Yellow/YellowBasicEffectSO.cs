
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "YellowBasicEffectSO", menuName = "Runes/Effects/YellowBasic")]
public class YellowBasicEffectSO : BaseRuneEffectSO
{
    /// <summary>
    /// 룬을 사용했을 때, runeValue만큼 골드를 획득합니다.
    /// targets(적)은 이 효과에서 사용되지 않습니다.
    /// </summary>
    public override void Execute(Player user, IEnumerable<Enemy> targets, int runeValue)
    {
        if (user == null)
        {
            Debug.LogError("YellowBasicEffectSO: user(Player)가 null이라 효과를 실행할 수 없습니다.");
            return;
        }

        // Player.cs에 있는 AddGold 함수를 호출하여 runeValue만큼 골드를 추가합니다.
        user.AddGold(runeValue);
        Debug.Log($"[YellowBasicEffect] 플레이어가 {runeValue} 골드를 획득했습니다.");
    }
}