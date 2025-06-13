// ConditionalDamageEffectSO.cs (수정된 최종본)

using UnityEngine;
using System.Collections.Generic;
using System.Linq; // ▼▼▼ 1. System.Linq using 추가 ▼▼▼

[CreateAssetMenu(fileName = "ConditionalDamageEffect", menuName = "Runes/Effects/Conditional Damage Effect")]
public class ConditionalDamageEffectSO : BaseRuneEffectSO
{
    [Header("화상일때 피해 설정")]
    [Tooltip("적이 화상(IsBurning) 상태일 때 입힐 피해량")]
    public int conditionalDamage = 5;

    [Header("기본 피해 설정")]
    [Tooltip("적이 화상 상태가 아닐 때 입힐 기본 피해량")]
    public int baseDamage = 2;

    // ▼▼▼ 2. 매개변수 타입을 List<Enemy>에서 IEnumerable<Enemy>로 수정 ▼▼▼
    public override void Execute(Player user, IEnumerable<Enemy> targets)
    {
        // ▼▼▼ 3. 비어있는지 확인하는 방식을 .Count == 0 에서 !Any()로 수정 ▼▼▼
        if (targets == null || !targets.Any())
        {
            Debug.LogWarning("ConditionalDamageEffectSO: 타겟이 지정되지 않아 효과를 실행할 수 없습니다.");
            return;
        }

        // 모든 타겟에게 효과 적용
        foreach (var target in targets)
        {
            if (target == null) continue;

            // 1단계에서 만든 IsBurning 속성을 사용하여 적의 화상 상태를 확인
            if (target.IsBurning)
            {
                // 화상 상태일 경우: conditionalDamage 만큼 피해
                Debug.Log($"{target.EnemyData.EnemyName}은(는) 화상 상태이므로 {conditionalDamage}의 강화된 피해를 입습니다.");
                target.Hit(conditionalDamage, user);
            }
            else
            {
                // 화상 상태가 아닐 경우: baseDamage 만큼 피해
                Debug.Log($"{target.EnemyData.EnemyName}은(는) 화상 상태가 아니므로 {baseDamage}의 기본 피해를 입습니다.");
                target.Hit(baseDamage, user);
            }
        }
    }
}