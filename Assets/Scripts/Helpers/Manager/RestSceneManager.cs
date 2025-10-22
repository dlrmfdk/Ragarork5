// RestSceneManager.cs (새로운 요구사항에 맞춘 버전)
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class RestSceneManager : MonoBehaviour
{
    [Header("UI 요소")]
    [SerializeField] private Button restButton;
    [SerializeField] private Button upgradeRuneButton;
    [SerializeField] private TMP_Text feedbackText;

    void Start()
    {
        // 각 버튼에 기능 연결
        restButton.onClick.AddListener(OnRestButtonClicked);
        upgradeRuneButton.onClick.AddListener(OnUpgradeRuneButtonClicked);

        // '룬 강화' 버튼은 아직 기능이 없으므로 비활성화하거나,
        // 눌렀을 때 "준비 중"이라는 메시지만 띄웁니다.
        // 여기서는 후자의 방식을 사용하겠습니다.

        // 피드백 텍스트는 처음엔 보이지 않도록 설정
        if (feedbackText != null)
        {
            feedbackText.gameObject.SetActive(false);
        }
    }

    private void OnRestButtonClicked()
    {
        if (Player.Instance == null)
        {
            Debug.LogError("Player.Instance를 찾을 수 없습니다!");
            return;
        }

        // 최대 체력의 30%를 계산 (소수점은 반올림)
        int healAmount = Mathf.RoundToInt(Player.Instance.MaxHealth * 0.3f);

        // Player의 Heal 메서드 호출
        Player.Instance.Heal(healAmount);

        // 피드백 메시지 표시
        ShowFeedback($"체력이 {healAmount}만큼 \n 회복되었습니다.");

        // 휴식은 한 번만 가능하도록 버튼 비활성화
        restButton.interactable = false;
    }

    private void OnUpgradeRuneButtonClicked()
    {
        // 아직 구현되지 않은 기능에 대한 안내
        ShowFeedback("룬 강화 기능은 현재 준비 중입니다.");
        Debug.Log("룬 강화 시스템은 추후 추가될 예정입니다.");
    }

    private void ShowFeedback(string message)
    {
        if (feedbackText != null)
        {
            feedbackText.text = message;
            feedbackText.gameObject.SetActive(true);
        }
    }
}