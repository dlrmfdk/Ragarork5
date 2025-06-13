// RuneTestHelper.cs (수정된 최종본)
using UnityEngine;
using System.Collections.Generic;

public class RuneTestHelper : MonoBehaviour
{
    [Header("알파 1키 테스트 설정")]
    [Tooltip("알파 1 키를 눌렀을 때 테스트할 룬 효과 ScriptableObject를 여기에 할당하세요.")]
    public BaseRuneEffectSO effectToTestOnAlpha1;

    [Header("알파 2키 테스트 설정")]
    [Tooltip("알파 2 키를 눌렀을 때 테스트할 룬 효과 ScriptableObject를 여기에 할당하세요.")]
    public BaseRuneEffectSO effectToTestOnAlpha2;

    void Update()
    {
#if UNITY_EDITOR
        if (Input.GetKeyDown(KeyCode.Alpha1))
        {
            TestSpecificEffect(effectToTestOnAlpha1, "알파 1");
        }
        if (Input.GetKeyDown(KeyCode.Alpha2))
        {
            TestSpecificEffect(effectToTestOnAlpha2, "알파 2");
        }
        if (Input.GetKeyDown(KeyCode.Alpha3))
        {
            if (TurnManager.Inst != null)
            {
                Debug.Log("[Rune Test Key - 알파 3] 현재 턴을 종료하고 다음 턴으로 진행합니다.");
                TurnManager.Inst.EndTurn();
            }
        }
#endif
    }

    // TestSpecificEffect와 TestSpecificEffect2 함수가 동일하므로 하나로 합쳤습니다.
    private void TestSpecificEffect(BaseRuneEffectSO effectSO, string keyName)
    {
        if (effectSO == null)
        {
            Debug.LogWarning($"[Rune Test Key - {keyName}] 테스트할 효과(EffectSO)가 할당되지 않았습니다.");
            return;
        }

        Player player = Player.Instance;
        // 씬에 있는 모든 적을 대상으로 하려면 FindObjectsOfType을 사용합니다.
        Enemy[] allEnemiesInScene = FindObjectsOfType<Enemy>();

        if (player == null)
        {
            Debug.LogWarning($"[Rune Test Key - {keyName}] Player 인스턴스를 찾을 수 없습니다.");
            return;
        }

        if (allEnemiesInScene.Length == 0)
        {
            Debug.LogWarning($"[Rune Test Key - {keyName}] 테스트 대상 Enemy를 씬에서 찾을 수 없습니다.");
            return;
        }

        Debug.Log($"[Rune Test Key - {keyName}] '{effectSO.name}' 효과 테스트 실행!");

        // ▼▼▼ 이 부분을 수정합니다 ▼▼▼
        // Execute 함수에 세 번째 인자로 테스트용 임의의 값(예: 5)을 추가합니다.
        effectSO.Execute(player, new List<Enemy>(allEnemiesInScene), 5);
    }
}