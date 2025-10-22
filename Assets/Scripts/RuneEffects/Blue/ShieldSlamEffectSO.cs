// ShieldSlamEffectSO.cs (수정된 최종본)
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "ShieldSlamEffect", menuName = "Runes/Effects/ShieldSlam")]
public class ShieldSlamEffectSO : BaseRuneEffectSO
{
    public override void Execute(Player user, IEnumerable<Enemy> targets, int runeValue)
    {
        if (user == null) return;

        // 1. 룬의 수치(runeValue)만큼 방어도를 먼저 얻습니다.
        user.IncreaseDefense(runeValue);

        // 2. [핵심] 방금 얻은 방어도를 포함한 '현재 총방어도' 수치를 가져옵니다.
        int damageAmount = RuneDeckManager.Instance.GetPredictedTotalDefense();

        if (damageAmount <= 0)
        {
            Debug.Log("[ShieldSlam] 방어도를 " + runeValue + " 얻었지만 총방어도가 0 이하라 공격하지 않습니다.");
            return;
        }

        if (targets == null)
        {
            Debug.LogWarning("[ShieldSlam] targets가 null입니다.");
            return;
        }

        // 이 로그에 찍히는 damageAmount가 실제 총방어도 수치인지 확인해보세요.
        Debug.Log($"[ShieldSlam] 방패 밀치기 발동! 현재 총방어도 {damageAmount}만큼 대상(들)을 공격합니다.");

        foreach (Enemy enemy in targets)
        {
            if (enemy != null && enemy.currentHealth > 0)
            {
                
                enemy.Hit(damageAmount, user);
                
            }
        }
    }
}