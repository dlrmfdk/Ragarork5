using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "InvestmentEffect", menuName = "Runes/Effects/Investment")]
public class InvestmentEffectSO : BaseRuneEffectSO
{
    /// <summary>
    /// runeValue만큼 골드를 얻습니다.
    /// 만약 (이 룬으로 얻기 전) 보유 골드가 100 이상이었다면,
    /// (얻기 전) 보유 골드 100당 10골드를 추가로 얻습니다.
    /// </summary>
    public override void Execute(Player user, IEnumerable<Enemy> targets, int runeValue)
    {
        // 사용자가 없으면 실행하지 않음
        if (user == null) return;

        // 1. 추가 골드 계산을 위해, 룬 효과 발동 '전'의 골드를 기록
        int goldBeforeEffect = user.Gold;
        Debug.Log($"[Investment] 시작. 현재 골드: {goldBeforeEffect}, 룬 수치: {runeValue}");

        // 2. 룬 수치(runeValue)만큼 기본 골드를 먼저 얻습니다.
        user.AddGold(runeValue);
        Debug.Log($"[Investment] 기본 골드 {runeValue} 획득. 현재 골드: {user.Gold}");

        // 3. 룬 효과 발동 '전' 골드가 100 이상이었는지 확인
        if (goldBeforeEffect >= 100)
        {
            // 4. 추가 골드 계산: (얻기 전 골드 / 100) * 10
            // 정수 나눗셈을 이용하면 '100 골드 당' 계산이 자동으로 됩니다. (예: 250 / 100 = 2)
            int bonusMultiplier = goldBeforeEffect / 100;
            int bonusGold = bonusMultiplier * 10;
            if (bonusGold > 100) bonusGold = 100;

            // 5. 계산된 추가 골드가 0보다 크면 지급
            if (bonusGold > 0)
            {
                user.AddGold(bonusGold);
                Debug.Log($"[Investment] 추가 골드 조건 만족! {bonusGold} 골드 (100골드당 10 * {bonusMultiplier}) 추가 획득. 최종 골드: {user.Gold}");
            }
            else
            {
                Debug.Log($"[Investment] 골드가 100 이상이었으나 추가 골드 계산 결과 0.");
            }
        }
        else
        {
            Debug.Log($"[Investment] 골드가 100 미만({goldBeforeEffect})이었으므로 추가 골드 없음. 최종 골드: {user.Gold}");
        }
    }
}