using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "BleedEffect", menuName = "Runes/Effects/BleedEffect")]
public class BleedEffectSO : BaseRuneEffectSO
{
    [SerializeField] private int bleedDuration = 3;

    public override void Execute(Player user, IEnumerable<Enemy> targets, int runeValue)
    {
        if (targets == null) return;

        // 턴당 피해량을 여기서 직접 계산합니다.
        // (float) 캐스팅으로 나눗셈이 소수점까지 계산되도록 하고, 그 결과를 반올림합니다.
        int damagePerTurn = Mathf.RoundToInt((float)runeValue / bleedDuration);
        if (damagePerTurn < 1) damagePerTurn = 1; // 최소 1의 피해 보장

        foreach (Enemy target in targets)
        {
            if (target != null)
            {
                // Enemy에게는 계산이 완료된 턴당 피해량을 전달합니다.
                target.ApplyBleed(damagePerTurn, this.bleedDuration);
            }
        }
    }
}