// LifestealAttackEffectSO.cs
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

[CreateAssetMenu(fileName = "LifestealAttackEffect", menuName = "Runes/Effects/LifestealAttackEffect")]
public class LifestealAttackEffectSO : BaseRuneEffectSO
{
    [Range(0f, 1f)]
    [SerializeField] private float lifestealPercentage = 0.1f;

    // MODIFIED: runeValue 파라미터를 추가합니다.
    public override void Execute(Player user, IEnumerable<Enemy> targets, int runeValue)
    {
        if (user == null || targets == null || !targets.Any()) return;

        int totalDamageDealt = 0;
        foreach (Enemy target in targets)
        {
            if (target != null)
            {
                // CHANGED: 플레이어의 공격력 대신 runeValue로 피해를 줍니다.
                int damageDealt = target.Hit(runeValue, user);
                totalDamageDealt += damageDealt;
            }
        }

        if (totalDamageDealt > 0)
        {
            int healAmount = Mathf.FloorToInt(totalDamageDealt * lifestealPercentage);
            if (healAmount > 0) user.Heal(healAmount);
        }
    }
}