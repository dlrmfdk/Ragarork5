
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "SelfHarmEffect", menuName = "Runes/Effects/SelfHarmEffect")]
public class SelfHarmEffectSO : BaseRuneEffectSO
{
    [SerializeField] private int selfDamage = 3;

    // 이 효과는 대상(targets)이 없으며, 사용자(user)에게만 적용됩니다. 
    public override void Execute(Player user, IEnumerable<Enemy> targets, int runeValue)
    {
        if (user != null)
        {
            Debug.Log($"자해의 룬 효과 발동! 플레이어가 {selfDamage}의 피해를 입습니다.");
            user.TakePureDamage(selfDamage); // 방어도를 무시하는 순수 피해
        }
    }
}