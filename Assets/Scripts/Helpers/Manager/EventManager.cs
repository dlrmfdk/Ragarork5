// EventManager.cs (확률 기능이 추가된 최종 버전)
using UnityEngine;
using TMPro;
using UnityEngine.UI;
using System.Collections.Generic;
using System.Linq; // Sum() 함수 사용을 위해 필요
using System.Text;

public class EventManager : MonoBehaviour
{
    public static EventManager Instance { get; private set; }

    [Header("이벤트 SO 목록")]
    [SerializeField] private List<EventSO> eventPool;

    [Header("UI 요소")]
    [SerializeField] private GameObject eventPanel;
    [SerializeField] private Image eventImage;
    [SerializeField] private TextMeshProUGUI descriptionText;
    [SerializeField] private Transform choiceButtonContainer;
    [SerializeField] private GameObject choiceButtonPrefab;
    [SerializeField] private TextMeshProUGUI outcomeText;
    [SerializeField] private Button continueButton;

    private void Awake() { /* 싱글톤 로직 */ }
    private void Start() { TriggerRandomEvent(); }

    public void TriggerRandomEvent()
    {
        if (eventPool == null || eventPool.Count == 0) return;
        EventSO randomEvent = eventPool[Random.Range(0, eventPool.Count)];
        DisplayEvent(randomEvent);
    }

    private void DisplayEvent(EventSO eventData)
    {
        // (이 함수는 기존과 거의 동일합니다)
        eventPanel.SetActive(true);
        outcomeText.gameObject.SetActive(false);
        continueButton.gameObject.SetActive(false);

        foreach (Transform child in choiceButtonContainer) Destroy(child.gameObject);

        eventImage.sprite = eventData.eventImage;
        descriptionText.text = eventData.eventDescription;

        foreach (var choice in eventData.choices)
        {
            GameObject buttonGO = Instantiate(choiceButtonPrefab, choiceButtonContainer);
            buttonGO.GetComponentInChildren<TextMeshProUGUI>().text = choice.choiceDescription;
            EventChoice currentChoice = choice;
            buttonGO.GetComponent<Button>().onClick.AddListener(() => OnChoiceSelected(currentChoice));
        }
    }

    // ▼▼▼ [핵심 수정] 선택에 따른 결과 처리 로직 ▼▼▼
    private void OnChoiceSelected(EventChoice choice)
    {
        foreach (Transform child in choiceButtonContainer)
        {
            child.GetComponent<Button>().interactable = false;
        }

        StringBuilder resultMessage = new StringBuilder();

        // 1. 확정적인 결과들을 먼저 모두 실행합니다.
        if (choice.guaranteedOutcomes != null)
        {
            foreach (var outcome in choice.guaranteedOutcomes)
            {
                ExecuteOutcome(outcome);
                if (!string.IsNullOrEmpty(outcome.outcomeMessage))
                {
                    resultMessage.AppendLine(outcome.outcomeMessage);
                }
            }
        }

        // 2. 확률적인 결과들 중 하나를 추첨하여 실행합니다.
        if (choice.randomOutcomes != null && choice.randomOutcomes.Count > 0)
        {
            // 모든 확률 가중치의 합을 구합니다.
            int totalWeight = choice.randomOutcomes.Sum(o => o.weight);
            // 0부터 총 가중치 합까지의 숫자 중 하나를 무작위로 뽑습니다.
            int randomRoll = Random.Range(0, totalWeight);

            // 추첨 시작
            foreach (var randomOutcome in choice.randomOutcomes)
            {
                if (randomRoll < randomOutcome.weight)
                {
                    // 뽑힌 결과를 실행하고 추첨을 종료합니다.
                    ExecuteOutcome(randomOutcome.outcome);
                    if (!string.IsNullOrEmpty(randomOutcome.outcome.outcomeMessage))
                    {
                        resultMessage.AppendLine(randomOutcome.outcome.outcomeMessage);
                    }
                    break;
                }
                // 뽑히지 않았다면, 다음 결과를 확인하기 위해 현재 결과의 가중치만큼 뺍니다.
                randomRoll -= randomOutcome.weight;
            }
        }

        // 3. 최종 결과 메시지를 표시하고 계속하기 버튼을 활성화합니다.
        outcomeText.text = resultMessage.ToString();
        outcomeText.gameObject.SetActive(true);
        continueButton.gameObject.SetActive(true);
        // continueButton.onClick.AddListener(...); // 맵으로 돌아가는 기능 연결
    }

    // ExecuteOutcome 함수는 기존과 동일합니다.
    private void ExecuteOutcome(EventOutcome outcome)
    {
        switch (outcome.type)
        {
            case EventOutcomeType.GainHealth: Player.Instance.Heal(outcome.amount); break;
            case EventOutcomeType.LoseHealth: Player.Instance.TakeDamage(outcome.amount); break;
            // ... (기타 다른 결과 처리) ...
            case EventOutcomeType.UpgradeRandomRune:
                // RuneDeckManager.Instance.UpgradeRandomRune(); // 룬 덱 매니저에 관련 기능 필요
                Debug.Log("무작위 룬 하나를 강화합니다.");
                break;
            case EventOutcomeType.Nothing: break;
        }
    }
}