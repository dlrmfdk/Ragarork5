using System.Collections;
using System.Collections.Generic;
using UnityEngine;


/// <summary>
/// 단일 행동(예: Draw 버튼 클릭) 동안의 전투 관련 데이터를 임시로 저장하는 static 클래스입니다.
/// </summary>
public static class BattleContext
{
    /// <summary>
    /// 현재 행동으로 인해 발생한 총 피해량을 기록합니다.
    /// </summary>
    public static int TotalDamageDealtThisAction { get; private set; }

    /// <summary>
    /// 새로운 행동이 시작되기 전에 호출되어 피해량 기록을 초기화합니다.
    /// </summary>
    public static void Reset()
    {
        TotalDamageDealtThisAction = 0;
    }

    /// <summary>
    /// 적이 피해를 입을 때마다 이 함수를 호출하여 피해량을 누적합니다.
    /// </summary>
    public static void AddDamage(int damage)
    {
        TotalDamageDealtThisAction += damage;
    }
}