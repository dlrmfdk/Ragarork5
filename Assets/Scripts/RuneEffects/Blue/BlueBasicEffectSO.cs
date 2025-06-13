
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "BlueBasicEffectSO", menuName = "Runes/Effects/BlueBasic")]
public class BlueBasicEffectSO : BaseRuneEffectSO
{
 
    // Execute 메서드가 runeValue를 받도록 시그니처를 수정합니다.
    public override void Execute(Player user, IEnumerable<Enemy> targets, int runeValue)
    {
        // 고정된 수치 대신, 매개변수로 받은 runeValue만큼 방어도를 올립니다.
        user.IncreaseDefense(runeValue);
    }
}