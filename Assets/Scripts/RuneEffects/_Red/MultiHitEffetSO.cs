// MultiHitEffetSO.cs
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

[CreateAssetMenu(fileName = "MultiHitEffect", menuName = "Runes/Effects/MultiHitEffect")]
public class MultiHitEffectSO : BaseRuneEffectSO
{
    [SerializeField] private int numberOfHits = 2;
    [SerializeField] private float damageMultiplierPerHit = 0.5f;
    [SerializeField] private float delayBetweenHits = 0.3f;

    // MODIFIED: runeValue 파라미터를 추가합니다.
    public override void Execute(Player user, IEnumerable<Enemy> targets, int runeValue)
    {
        if (user == null || targets == null || !targets.Any()) return;

        // CHANGED: 플레이어 공격력 대신 runeValue를 기반으로 피해량을 계산합니다.
        int damagePerHit = Mathf.FloorToInt(runeValue * damageMultiplierPerHit);
        if (damagePerHit <= 0 && runeValue > 0) damagePerHit = 1;

        user.PerformMultiHit(targets, damagePerHit, numberOfHits, delayBetweenHits);
    }
}