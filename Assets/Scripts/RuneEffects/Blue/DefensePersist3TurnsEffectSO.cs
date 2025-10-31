using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "DefensePersist3TurnsEffect", menuName = "Runes/Effects/DefensePersist3Turns")]
public class DefensePersist3TurnsEffectSO : BaseRuneEffectSO
{
    private const int DURATION = 1; // 방어도가 유지될 턴 수

    public override void Execute(Player user, IEnumerable<Enemy> targets, int runeValue)
    {
        if (user == null) return;

        // 1. 룬 수치만큼 기본 방어도를 얻습니다.
        user.IncreaseDefense(runeValue);

        // 2. Player에게 방어도를 '3턴' 동안 유지하도록 설정합니다.
        user.SetDefenseCarryOver(DURATION);
        Debug.Log($"[DefensePersist] 방어도를 {runeValue} 얻고, 다음 {DURATION}턴 동안 유지합니다.");
    }
}