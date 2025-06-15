
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

[CreateAssetMenu(fileName = "LifestealAttackEffect", menuName = "Runes/Effects/LifestealAttackEffect")]
public class LifestealAttackEffectSO : BaseRuneEffectSO
{
    [Range(0f, 1f)]
    [SerializeField] private float lifestealPercentage = 0.2f; // 20%

    public override void Execute(Player user, IEnumerable<Enemy> targets, int runeValue)
    {
        if (user == null || targets == null || !targets.Any()) return;

        int totalDamageDealt = 0;
        foreach (Enemy target in targets)
        {
            if (target != null)
            {
                int damageDealt = target.Hit(runeValue, user);
                totalDamageDealt += damageDealt;
            }
        }

        if (totalDamageDealt > 0)
        {
            // CHANGED: Mathf.FloorToInt 대신 Mathf.RoundToInt를 사용하여 반올림합니다.
            int healAmount = Mathf.RoundToInt(totalDamageDealt * lifestealPercentage);
            if (healAmount > 0) user.Heal(healAmount);
        }
    }
}