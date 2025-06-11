using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "BlueBasicEffectSO", menuName = "Runes/Effects/BlueBasic")]
public class BlueBasicEffectSO : BaseRuneEffectSO
{
    [Header("플레이어가 얻을 방어도")]
    public int defenseAmount = 5;

    /// <summary>
    /// 룬을 사용했을 때 호출됩니다.
    /// targets는 무시하고, user(플레이어)에게만 방어도를 부여합니다.
    /// </summary>
    public override void Execute(Player user, IEnumerable<Enemy> targets)
    {
        user.IncreaseDefense(defenseAmount);
    }
}