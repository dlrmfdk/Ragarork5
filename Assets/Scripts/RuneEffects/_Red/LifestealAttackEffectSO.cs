using System.Collections.Generic;
using System.Linq;
using UnityEngine;

[CreateAssetMenu(fileName = "LifestealAttackEffect", menuName = "Runes/Effects/LifestealAttackEffect")]
public class LifestealAttackEffectSO : BaseRuneEffectSO
{
    [Header("흡혈 설정")]
    [Range(0f, 1f)] // 0% ~ 100%
    [SerializeField] private float lifestealPercentage = 0.1f; // 10%

    /// <summary>
    /// 플레이어의 공격력으로 대상을 공격하고, 입힌 총 피해량의 일정 비율만큼 체력을 회복합니다.
    /// </summary>
    public override void Execute(Player user, IEnumerable<Enemy> targets)
    {
        if (user == null || targets == null || !targets.Any())
        {
            Debug.LogWarning("LifestealAttackEffectSO: 사용자 또는 타겟이 없어 효과를 실행할 수 없습니다.");
            return;
        }

        int totalDamageDealt = 0;
        // Player.cs에 attackPower 필드 또는 GetAttackPower()와 같은 메소드가 있다고 가정합니다.
        // 현재 Player.cs 스크립트를 기준으로 attackPower를 사용합니다.
        int baseAttackPower = user.AttackPower;

        // 이 룬은 공격 룬이므로, 전달된 모든 타겟을 공격합니다.
        // (단일 타겟팅 시스템과 함께 사용되면 targets 리스트에는 적이 한 마리만 있게 됩니다.)
        foreach (Enemy target in targets)
        {
            if (target != null)
            {
                // 적을 공격하고, 실제 입힌 피해량을 반환받습니다.
                int damageDealt = target.Hit(baseAttackPower, user);
                totalDamageDealt += damageDealt;
            }
        }

        if (totalDamageDealt > 0)
        {
            // 입힌 총 피해량에 비례하여 회복량 계산
            int healAmount = Mathf.FloorToInt(totalDamageDealt * lifestealPercentage);
            if (healAmount > 0)
            {
                Debug.Log($"총 {totalDamageDealt}의 피해를 입히고, {lifestealPercentage * 100}%인 {healAmount}만큼 흡혈합니다.");
                // Player.cs의 Heal 함수를 호출합니다.
                user.Heal(healAmount);
            }
        }
        else
        {
            Debug.Log("입힌 피해가 없어 흡혈 효과가 발동하지 않았습니다.");
        }
    }
}