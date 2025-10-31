using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "GoldenGuardEffect", menuName = "Runes/Effects/GoldenGuard")]
public class GoldenGuardEffectSO : BaseRuneEffectSO
{
    /// <summary>
    /// 현재 손패에 있는 모든 노란색 룬의 Value 합계만큼 방어도를 얻습니다.
    /// 이 룬 자체의 runeValue는 효과 계산에 사용되지 않습니다.
    /// </summary>
    public override void Execute(Player user, IEnumerable<Enemy> targets, int runeValue)
    {
        // 사용자가 없거나 룬 덱 매니저가 없으면 실행하지 않음
        if (user == null || RuneDeckManager.Instance == null) return;

        // 1. RuneDeckManager에게 현재 손패의 '예측 총 골드(노랑 룬 합계)'를 물어봅니다.
        int defenseAmount = RuneDeckManager.Instance.GetPredictedTotalGold();

        // 2. 계산된 방어도가 0보다 크면 방어도를 올립니다.
        if (defenseAmount > 0)
        {
            Debug.Log($"[GoldenGuard] 발동! 손패의 노랑 룬 합계({defenseAmount})만큼 방어도를 얻습니다.");
            user.IncreaseDefense(defenseAmount);
        }
        else
        {
            Debug.Log("[GoldenGuard] 손패에 노랑 룬이 없어 방어도를 얻지 못했습니다.");
        }
    }
}