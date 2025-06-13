// BattleContext.cs
public static class BattleContext
{
    /// <summary>
    /// 현재 행동으로 입힌 총 피해량을 저장합니다.
    /// </summary>
    public static int TotalDamageDealtThisAction { get; set; }

    /// <summary>
    /// 새로운 행동이 시작되기 전에 호출되어 피해량 카운터를 초기화합니다.
    /// </summary>
    public static void Reset()
    {
        TotalDamageDealtThisAction = 0;
    }
}