using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "RecklessStrikeEffect", menuName = "Runes/Effects/RecklessStrike")]
public class RecklessStrikeEffectSO : BaseRuneEffectSO
{
    /// <summary>
    /// 대상에게 runeValue의 3배만큼 피해를 주고,
    /// 사용자(플레이어)는 runeValue만큼 방어도를 무시하는 피해를 입습니다.
    /// </summary>
    public override void Execute(Player user, IEnumerable<Enemy> targets, int runeValue)
    {
        // 사용자나 대상이 없으면 실행하지 않음
        if (user == null || targets == null) return;

        // 1. 대상(들)에게 강력한 피해 주기
        int damageToEnemy = runeValue * 3; // 룬 수치의 3배 계산
        foreach (Enemy target in targets)
        {
            if (target != null && target.currentHealth > 0)
            {
                Debug.Log($"[RecklessStrike] {target.name}에게 {damageToEnemy} 피해!");
                target.Hit(damageToEnemy, user); // 일반 Hit 함수 사용
            }
        }

        // 2. 사용자(플레이어)에게 반동 피해 주기
        if (runeValue > 0) // 피해량이 0보다 클 때만 반동 피해
        {
            Debug.Log($"[RecklessStrike] 반동으로 플레이어가 {runeValue}의 순수 피해!");
            user.TakePureDamage(runeValue); // 방어도 무시 피해 함수 사용
        }
    }
}