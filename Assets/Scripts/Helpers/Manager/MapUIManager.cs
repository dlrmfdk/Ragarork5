using UnityEngine;
using TMPro; // TextMeshPro를 사용하기 위해 필요

public class MapUIManager : MonoBehaviour
{
    [Header("UI 요소")]
    [SerializeField] private TextMeshProUGUI goldText; // 맵 씬에서 골드를 표시할 텍스트 UI

    // 이 씬이 활성화될 때마다 (예: 전투 후 돌아올 때) 호출됩니다.
    void OnEnable()
    {
        UpdateGoldDisplay();
    }

    // (Start에서도 한번 더 호출해 주면 안전합니다)
    void Start()
    {
        UpdateGoldDisplay();
    }

    /// <summary>
    /// Player 인스턴스로부터 현재 골드 정보를 가져와 UI 텍스트를 업데이트합니다.
    /// </summary>
    private void UpdateGoldDisplay()
    {
        // 1. 골드 텍스트 UI가 연결되었는지 확인
        if (goldText == null)
        {
            Debug.LogError("MapUIManager: 'goldText'가 인스펙터에 연결되지 않았습니다!");
            return;
        }

        // 2. Player 인스턴스가 존재하는지 확인
        if (Player.Instance != null)
        {
            // 3. Player의 골드 값을 가져와 텍스트로 표시
            goldText.text = Player.Instance.Gold.ToString();
        }
        else
        {
            // 4. Player 인스턴스를 찾을 수 없는 경우 (예: 맵 씬에서 바로 테스트 실행 시)
            Debug.LogWarning("MapUIManager: Player.Instance를 찾을 수 없습니다. (테스트 실행일 수 있습니다)");
            goldText.text = "0"; // 기본값 0으로 표시
        }
    }
}