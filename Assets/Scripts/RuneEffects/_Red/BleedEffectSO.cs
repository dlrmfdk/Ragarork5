using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "BleedEffect", menuName = "Runes/Effects/BleedEffect")]
public class BleedEffectSO : BaseRuneEffectSO
{
    [Header("출혈 설정")]
    [SerializeField] private int totalBleedDamage = 6; // 총 피해량
    [SerializeField] private int bleedDuration = 3;    // 지속 턴 수

    /// <summary>
    /// 대상에게 설정된 값으로 출혈 효과를 부여합니다.
    /// </summary>
    public override void Execute(Player user, IEnumerable<Enemy> targets)
    {
        Debug.Log("출혈 부여 효과 발동!");

        if (targets == null)
        {
            Debug.LogWarning("BleedEffectSO: 대상(targets)이 null입니다.");
            return;
        }

        foreach (Enemy target in targets)
        {
            if (target != null)
            {
                Debug.Log($"{target.name}에게 {bleedDuration}턴 동안 총 {totalBleedDamage}의 출혈을 부여합니다.");
                target.ApplyBleed(this.totalBleedDamage, this.bleedDuration);
            }
        }
    }
}