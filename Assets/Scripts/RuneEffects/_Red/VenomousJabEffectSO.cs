using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "VenomousJabEffect", menuName = "Runes/Effects/VenomousJab")]
public class VenomousJabEffectSO : BaseRuneEffectSO
{
    [Header("독 효과 설정")]
    [Tooltip("이 룬으로 부여할 고정 독 수치입니다.")]
    [SerializeField] private int poisonAmount = 2; // 독 2 부여

    /// <summary>
    /// 대상에게 runeValue만큼 피해를 주고, 고정 수치(poisonAmount)만큼 독을 부여합니다.
    /// </summary>
    public override void Execute(Player user, IEnumerable<Enemy> targets, int runeValue)
    {
        // 사용자나 대상이 없으면 실행하지 않음
        if (user == null || targets == null) return;

        foreach (Enemy target in targets)
        {
            if (target != null && target.currentHealth > 0)
            {
                // 1. 룬 수치(runeValue)만큼 기본 피해를 줍니다.
                target.Hit(runeValue, user);

                // 2. 고정 수치(poisonAmount)만큼 독을 부여합니다.
                target.ApplyPoison(poisonAmount);
                Debug.Log($"[VenomousJab] {target.name}에게 {runeValue} 피해 및 독 {poisonAmount} 부여.");
            }
        }
    }
}