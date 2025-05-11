using System.Collections.Generic;
using UnityEngine;

public abstract class BaseRuneEffectSO : ScriptableObject
{
    /// <summary>
    /// 룬이 실행될 때 호출됩니다.
    /// </summary>
    /// <param name="user">룬을 사용한 주체 (플레이어)</param>
    /// <param name="targets">효과 대상(들) (적 목록 등)</param>
    public abstract void Execute(Player user, IEnumerable<Enemy> targets);
}
