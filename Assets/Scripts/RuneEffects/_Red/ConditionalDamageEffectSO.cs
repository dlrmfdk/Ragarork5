// ConditionalDamageEffectSO.cs
using UnityEngine;
using System.Collections.Generic;
using System.Linq;

// 이전에 만들었던 '방패 뚫기'용 ShieldPierceEffectSO.cs도 이와 동일한 방식으로 수정해주세요.
[CreateAssetMenu(fileName = "ConditionalDamageEffect", menuName = "Runes/Effects/Conditional Damage Effect")]
public class ConditionalDamageEffectSO : BaseRuneEffectSO
{
    public enum ConditionType { IsBurning, HasArmor }
    public ConditionType condition;
    public int baseDamage = 2;

    // MODIFIED: runeValue 파라미터를 추가합니다.
    public override void Execute(Player user, IEnumerable<Enemy> targets, int runeValue)
    {
        if (targets == null || !targets.Any()) return;

        foreach (var target in targets)
        {
            if (target == null) continue;
            bool conditionMet = (condition == ConditionType.IsBurning) ? target.IsBurning : target.CurrentArmor > 0;

            if (conditionMet)
            {
                // CHANGED: 고정된 conditionalDamage 대신 runeValue를 사용합니다.
                target.Hit(runeValue+5, user);
            }
            else
            {
                target.Hit(runeValue, user);
            }
        }
    }
}