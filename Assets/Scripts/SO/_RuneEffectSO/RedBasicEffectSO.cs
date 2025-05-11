using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "RedBasicEffect", menuName = "Runes/Effects/RedBasic")]
public class RedBasicEffectSO : BaseRuneEffectSO
{
    [Header("피해량")]
    public int damage = 3;

    public override void Execute(Player user, IEnumerable<Enemy> targets)
    {
        // targets 가 변경돼도 안전하도록 사본 사용
        List<Enemy> snapshot = new List<Enemy>(targets);
        foreach (var e in snapshot)
            e.Hit(damage, user);
    }
}
