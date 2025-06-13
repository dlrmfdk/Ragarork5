
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "BleedEffect", menuName = "Runes/Effects/BleedEffect")]
public class BleedEffectSO : BaseRuneEffectSO
{
    [SerializeField] private int bleedDuration = 3; // 지속 턴 수는 유지

    // MODIFIED: runeValue 파라미터를 추가합니다.
    public override void Execute(Player user, IEnumerable<Enemy> targets, int runeValue)
    {
        if (targets == null) return;

        foreach (Enemy target in targets)
        {
            if (target != null)
            {
                // CHANGED: 고정된 totalBleedDamage 대신 runeValue를 사용합니다.
                target.ApplyBleed(runeValue, this.bleedDuration);
            }
        }
    }
}