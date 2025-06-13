
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "RedBasicEffect", menuName = "Runes/Effects/RedBasic")]
public class RedBasicEffectSO : BaseRuneEffectSO
{
   

    // runeValue 매개변수를 추가하여 override 합니다.
    public override void Execute(Player user, IEnumerable<Enemy> targets, int runeValue)
    {
        List<Enemy> snapshot = new List<Enemy>(targets);
        foreach (var e in snapshot)
        {
            // 고정된 damage 대신, 매개변수로 받은 runeValue를 사용합니다.
            e.Hit(runeValue, user);
        }
    }
}