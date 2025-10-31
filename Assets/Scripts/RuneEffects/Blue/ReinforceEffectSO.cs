using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "ReinforceEffect", menuName = "Runes/Effects/Reinforce")]
public class ReinforceEffectSO : BaseRuneEffectSO
{
    [Header("추가 방어도 설정")]
    [Tooltip("기존 방어도가 있을 때 추가로 얻는 방어도 수치입니다.")]
    [SerializeField] private int bonusDefense = 2; // 추가 방어도 +2

    public override void Execute(Player user, IEnumerable<Enemy> targets, int runeValue)
    {
        if (user == null) return;

        // 1. 룬 효과 발동 '전'의 방어도를 확인합니다.
        int defenseBeforeEffect = user.CurrentDefense;

        // 2. 룬 수치(runeValue)만큼 기본 방어도를 얻습니다.
        int defenseToGain = runeValue;

        // 3. 만약 효과 발동 전에 방어도가 0보다 컸다면, 추가 방어도를 더합니다.
        if (defenseBeforeEffect > 0)
        {
            defenseToGain *= bonusDefense;
            Debug.Log($"[Reinforce] 기존 방어도가 {defenseBeforeEffect} 있어 추가 방어도 {bonusDefense} 적용!");
        }

        // 4. 최종 계산된 방어도를 적용합니다.
        user.IncreaseDefense(defenseToGain);
        Debug.Log($"[Reinforce] 총 {defenseToGain} 방어도를 얻었습니다. (기본 {runeValue} + 보너스 {(defenseBeforeEffect > 0 ? bonusDefense : 0)})");
    }
}