
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

[CreateAssetMenu(fileName = "MultiHitEffect", menuName = "Runes/Effects/MultiHitEffect")]
public class MultiHitEffectSO : BaseRuneEffectSO
{
    [SerializeField] private int numberOfHits = 2;
    [SerializeField] private float damageMultiplierPerHit = 0.5f;
    [SerializeField] private float delayBetweenHits = 0.3f;

    public override void Execute(Player user, IEnumerable<Enemy> targets, int runeValue)
    {
        if (user == null || targets == null || !targets.Any()) return;

        // CHANGED: Mathf.FloorToInt 대신 Mathf.RoundToInt를 사용하여 반올림합니다.
        int damagePerHit = Mathf.RoundToInt(runeValue * damageMultiplierPerHit);
        if (damagePerHit <= 0 && runeValue > 0) damagePerHit = 1;

        user.PerformMultiHit(targets, damagePerHit, numberOfHits, delayBetweenHits);
    }
}