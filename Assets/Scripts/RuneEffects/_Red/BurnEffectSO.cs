using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "Red_BurnRuneEffect", menuName = "Runes/Effects/BurnEffect")]
public class BurnEffectSO : BaseRuneEffectSO
{
    [Header("화상 설정")]
    [SerializeField] private int burnDamagePerTurn = 2; // 룬 효과: 턴당 2 데미지
    [SerializeField] private int burnDuration = 2;      // 룬 효과: 2턴 지속

    /// <summary>
    /// 대상에게 설정된 값으로 화상 효과를 부여합니다.
    /// </summary>
    public override void Execute(Player user, IEnumerable<Enemy> targets)
    {
        Debug.Log("화상 부여 효과 발동!");

        if (targets == null)
        {
            Debug.LogWarning("BurnEffectSO: 대상(targets)이 null입니다.");
            return;
        }

        foreach (Enemy target in targets)
        {
            if (target == null)
            {
                Debug.LogWarning("BurnEffectSO: targets 리스트 내에 null인 적이 포함되어 있습니다.");
                continue;
            }

            Debug.Log($"{target.name}에게 {burnDuration}턴 동안 매 턴 {burnDamagePerTurn}의 화상을 부여합니다.");
            target.ApplyBurn(this.burnDamagePerTurn, this.burnDuration);
        }
    }
}