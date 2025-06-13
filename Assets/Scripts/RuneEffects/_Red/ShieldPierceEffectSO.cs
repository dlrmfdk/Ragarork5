// ShieldPierceEffectSO.cs

using UnityEngine;
using System.Collections.Generic;
using System.Linq;

[CreateAssetMenu(fileName = "ShieldPierceEffectSO", menuName = "Runes/Effects/Shield Pierce Effect")]
public class ShieldPierceEffectSO : BaseRuneEffectSO
{
    [Header("피해량 설정")]
    [Tooltip("적이 방어도를 가지고 있을 때 입힐 피해량")]
    public int pierceDamage = 5;

    [Tooltip("적이 방어도가 없을 때 입힐 기본 피해량")]
    public int baseDamage = 2;

    public override void Execute(Player user, IEnumerable<Enemy> targets)
    {
        if (targets == null || !targets.Any())
        {
            Debug.LogWarning("ShieldPierceEffectSO: 타겟이 지정되지 않아 효과를 실행할 수 없습니다.");
            return;
        }

        foreach (var target in targets)
        {
            if (target == null) continue;

            // 적의 방어도 상태를 확인 (이전에 Enemy.cs에 추가한 CurrentArmor 속성 사용)
            if (target.CurrentArmor > 0)
            {
                Debug.Log($"{target.EnemyData.EnemyName}은(는) 방어도가 있으므로 {pierceDamage}의 관통 피해를 입습니다.");
                target.Hit(pierceDamage, user);
            }
            else
            {
                Debug.Log($"{target.EnemyData.EnemyName}은(는) 방어도가 없으므로 {baseDamage}의 기본 피해를 입습니다.");
                target.Hit(baseDamage, user);
            }
        }
    }
}