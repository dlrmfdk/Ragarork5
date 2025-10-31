using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "SmiteEffect", menuName = "Runes/Effects/Smite")]
public class SmiteEffectSO : BaseRuneEffectSO
{
    /// <summary>
    /// 대상에게 (runeValue + 현재 보유 골드 / 10) 만큼의 피해를 줍니다. (소수점 버림)
    /// </summary>
    /// 
    // ▼▼▼ 1. 골드 소모량 변수 추가 ▼▼▼
    [Header("골드 소모 설정")]
    [Tooltip("이 룬을 사용하기 위해 필요한 골드입니다.")]
    [SerializeField] private int goldCost = 0; // 예: 10 골드 소모
    // ▲▲▲ 추가 완료 ▲▲▲

    /// <summary>
    /// goldCost만큼 골드를 '소모'하고,
    /// 성공 시 (runeValue + 현재 보유 골드 / 10) 만큼의 피해를 줍니다.
    /// 골드가 부족하면 runeValue만큼의 기본 피해만 줍니다.
    /// </summary>
    public override void Execute(Player user, IEnumerable<Enemy> targets, int runeValue)
    {
        // 사용자나 대상이 없으면 실행하지 않음
        if (user == null || targets == null) return;

        // 1. 현재 플레이어의 골드 확인
        int currentGold = user.Gold;

        // 2. 추가 데미지 계산: (현재 골드 / 10)
        // 정수 나눗셈은 자동으로 소수점을 버립니다. (예: 99 / 10 = 9)
        int bonusDamage = currentGold / 100;

        // 3. 최종 데미지 계산: 룬 수치 + 추가 데미지
        int totalDamage = runeValue + bonusDamage;

        Debug.Log($"[Smite] 발동! 기본 피해({runeValue}) + 골드 보너스({bonusDamage}) = 총 {totalDamage} 피해.");
        goldCost = bonusDamage * 10;
        // ▼▼▼ 4. 골드 소모 시도 및 분기 처리 ▼▼▼
        // 4-A. 골드 소모 시도 (SpendGold 함수는 성공 시 true, 실패 시 false 반환)
        if (user.SpendGold(goldCost))
        {
            // [성공] 골드를 소모하고, 계산된 '전체 피해'를 줍니다.
            Debug.Log($"[Smite] 골드 {goldCost} 소모! 기본 피해({runeValue}) + 골드 보너스({bonusDamage}) = 총 {totalDamage} 피해.");
            foreach (Enemy target in targets)
            {
                if (target != null && target.currentHealth > 0)
                {
                    target.Hit(totalDamage, user);
                }
            }
        }
        else // 4-B. 골드가 부족하여 SpendGold(false) 반환
        {
            // [실패] 골드 소모 없이, '기본 피해'(runeValue)만 줍니다.
            Debug.Log($"[Smite] 골드가 부족하여 비용({goldCost}) 지불 실패! 기본 피해({runeValue})만 줍니다.");
            foreach (Enemy target in targets)
            {
                if (target != null && target.currentHealth > 0)
                {
                    target.Hit(runeValue, user);
                }
            }
        }
        // ▲▲▲ 수정 완료 ▲▲▲
        
    }
}