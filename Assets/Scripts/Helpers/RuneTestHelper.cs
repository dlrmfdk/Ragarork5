using UnityEngine;
using System.Collections.Generic; // List<Enemy>를 사용하기 위해 추가

public class RuneTestHelper : MonoBehaviour
{
    [Header("알파 1키 테스트 설정")]
    [Tooltip("알파 1 키를 눌렀을 때 테스트할 룬 효과 ScriptableObject를 여기에 할당하세요.")]
    public BaseRuneEffectSO effectToTestOnAlpha1; // 테스트할 효과 SO (인스펙터에서 할당)

    // 필요에 따라 다른 키에 대한 테스트 효과도 추가할 수 있습니다.
    // [Header("알파 2키 테스트 설정")]
    // public BaseRuneEffectSO effectToTestOnAlpha2;

    void Update()
    {
        // UNITY_EDITOR 전처리기는 이 코드가 유니티 에디터 내에서 실행될 때만 포함되도록 합니다.
        // 빌드된 게임에는 이 디버그 코드가 포함되지 않아 안전합니다.
        if (Input.GetKeyDown(KeyCode.Alpha1)) { TestDirectBurn(); } //알파 숫자 1 키를 눌렀을 때 직접 화상 부여 테스트
#if UNITY_EDITOR
        // 알파 숫자 1 키를 눌렀을 때
        if (Input.GetKeyDown(KeyCode.Alpha1))
        {
            TestSpecificEffect(effectToTestOnAlpha1, "알파 1");
        }
        // 알파 숫자 2 키를 눌렀을 때 (턴 넘기기)
        if (Input.GetKeyDown(KeyCode.Alpha2))
        {
            // --- 여기에 사용자의 턴 매니저 호출 ---
            // 예시: TurnManager 스크립트에 EndPlayerTurn() 또는 NextTurn() 같은 메소드가 있다고 가정합니다.
            // 실제 프로젝트의 턴 매니저 스크립트와 메소드명으로 변경해주세요.

            // 예시 1: TurnManager 라는 이름의 스크립트가 있고, 싱글톤 Instance를 제공하는 경우
            if (TurnManager.Inst != null) // TurnManager가 있는지 확인
            {
                Debug.Log("[Rune Test Key - 알파 2] 현재 턴을 종료하고 다음 턴으로 진행합니다.");
                TurnManager.Inst.EndTurn(); // 플레이어 턴 종료 메소드 호출 예시

            }
            else
            {
                Debug.LogWarning("[Rune Test Key - 알파 2] TurnManager 인스턴스를 찾을 수 없습니다. 턴을 넘길 수 없습니다.");
            }

#endif
        }
    }

    // 특정 효과를 테스트하는 공용 메소드
    private void TestSpecificEffect(BaseRuneEffectSO effectSO, string keyName)
    {
        if (effectSO == null)
        {
            Debug.LogWarning($"[Rune Test Key - {keyName}] 테스트할 효과(EffectSO)가 할당되지 않았습니다. RuneTestHelper 인스펙터에서 설정해주세요.");
            return;
        }

        Player player = Player.Instance; // 싱글톤 Player 인스턴스 가져오기
        Enemy enemy = FindObjectOfType<Enemy>(); // 씬에 있는 첫 번째 Enemy 객체를 찾습니다. (테스트용)

        if (player == null)
        {
            Debug.LogWarning($"[Rune Test Key - {keyName}] Player 인스턴스를 찾을 수 없습니다.");
            return;
        }

        if (enemy == null)
        {
            Debug.LogWarning($"[Rune Test Key - {keyName}] 테스트 대상 Enemy를 씬에서 찾을 수 없습니다.");
            return;
        }

        // 효과 실행
        // 여러 적에게 테스트하고 싶다면 new List<Enemy> { enemy1, enemy2, ... } 형태로 전달할 수 있습니다.
        // 현재는 찾은 첫 번째 적에게만 적용합니다.
        Debug.Log($"[Rune Test Key - {keyName}] 플레이어 '{player.name}'이(가) 적 '{enemy.name}'에게 '{effectSO.name}' 효과 테스트 실행!");
        effectSO.Execute(player, new List<Enemy> { enemy });
    }

    // 만약 Enemy에게 직접 특정 상태(예: 화상)를 부여하는 테스트를 하고 싶다면 아래와 같은 메소드를 추가할 수 있습니다.
    // (이 경우, effectToTestOnAlpha1 대신 직접 호출)
    
    private void TestDirectBurn()
    {
        Player player = Player.Instance;
        Enemy enemy = FindObjectOfType<Enemy>();

        if (player != null && enemy != null)
        {
            int testBurnDamage = 0;
            int testBurnDuration = 0;
            enemy.ApplyBurn(testBurnDamage, testBurnDuration);
            Debug.Log($"[Rune Test Key] 적 '{enemy.name}'에게 직접 화상 부여 테스트! (데미지: {testBurnDamage}, 지속시간: {testBurnDuration})");
        }
        else
        {
            Debug.LogWarning("[Rune Test Key] 플레이어 또는 적을 찾을 수 없어 직접 화상 부여 테스트를 실행할 수 없습니다.");
        }
    }
    
  
}