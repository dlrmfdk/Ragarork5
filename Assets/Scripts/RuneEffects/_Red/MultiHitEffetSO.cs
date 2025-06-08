using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

[CreateAssetMenu(fileName = "MultiHitEffect", menuName = "Runes/Effects/MultiHitEffect")]
public class MultiHitEffectSO : BaseRuneEffectSO
{
    [Header("연속타 설정")]
    [SerializeField] private int numberOfHits = 2;
    [SerializeField] private float damageMultiplierPerHit = 0.5f; // 50%
    [Tooltip("각 타격 사이의 시간 간격(초)")]
    [SerializeField] private float delayBetweenHits = 0.3f; // 타격 간 딜레이 추가

    /// <summary>
    /// Player에게 연속타 코루틴 실행을 요청합니다.
    /// </summary>
    public override void Execute(Player user, IEnumerable<Enemy> targets)
    {
        if (user == null || targets == null || !targets.Any())
        {
            Debug.LogWarning("MultiHitEffectSO: 사용자 또는 타겟이 없어 효과를 실행할 수 없습니다.");
            return;
        }

        int baseAttackPower = user.AttackPower; // Player.cs의 public 속성 사용
        int damagePerHit = Mathf.FloorToInt(baseAttackPower * damageMultiplierPerHit);

        if (damagePerHit <= 0 && baseAttackPower > 0)
        {
            damagePerHit = 1;
        }

        Debug.Log($"연속타 효과 발동 요청! 1회당 피해량: {damagePerHit}, 타격 횟수: {numberOfHits}, 타격 간 딜레이: {delayBetweenHits}초");

        // Player.cs (MonoBehaviour)에게 코루틴 실행을 위임합니다.
        user.PerformMultiHit(targets, damagePerHit, numberOfHits, delayBetweenHits);
    }
}