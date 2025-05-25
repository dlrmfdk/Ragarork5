// RewardManager.cs 수정 제안
using System.Collections.Generic;
using System.Linq; // Random 선택 시 중복 방지를 위해 필요할 수 있음
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class RewardManager : MonoBehaviour
{
    public static RewardManager Instance { get; private set; }

    [Header("메인 보상 패널")]
    [SerializeField] private GameObject RewardPanel;
    [SerializeField] private Button RewardRuneButton;
    [SerializeField] private Button RewardGoldButton;

    [Header("다음 진행 버튼")]
    [SerializeField] private Button mapButton;

    [Header("룬 보상 UI")]
    [SerializeField] private GameObject RuneRewardPanel;
    [SerializeField] private Button[] OptionButtons;

    [Header("보상 룬 SO 목록 (Inspector에서 채워주세요)")]
    [SerializeField] private List<RuneSO> RewardPool;

    // [SerializeField] private RuneDeckManager deckManager; // RuneDeckManager.Instance 사용

    private bool hasClaimedRuneReward = false; // 이번 보상 단계에서 룬 보상을 이미 받았는지 여부
    private bool hasClaimedGoldReward = false; // 이번 보상 단계에서 골드 보상을 이미 받았는지 여부

    // 이 메서드는 현재 사용되지 않거나 OnRuneOptionChosen과 역할이 중복될 수 있습니다.
    // 만약 다른 곳에서 호출된다면 해당 호출 경로를 확인하고 로직을 정리해야 합니다.
    public void OnRuneRewardChosen(string rewardRuneID)
    {
        Debug.LogWarning($"[RewardManager] OnRuneRewardChosen 호출됨 (룬 ID: {rewardRuneID}). " +
                         "이 메서드는 OnRuneOptionChosen과 기능이 중복될 수 있으므로 호출 경로 및 역할 확인 필요.");
        // 현재 설계상 이 메서드는 직접적인 덱 변경 로직을 수행하지 않아야 합니다.
        // 모든 덱 변경은 OnRuneOptionChosen에서 단일 책임으로 처리합니다.
    }

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
            return;
        }

        // 리스너 등록은 Start 또는 OnEnable에서 하는 것이 더 안전할 수 있지만, Awake도 일반적입니다.
        // 확실한 것은 RemoveAllListeners 후에 AddListener를 하는 것입니다.
        if (RewardRuneButton != null)
        {
            RewardRuneButton.onClick.RemoveAllListeners();
            RewardRuneButton.onClick.AddListener(OpenRuneRewardPanel);
        }
        if (RewardGoldButton != null)
        {
            RewardGoldButton.onClick.RemoveAllListeners();
            RewardGoldButton.onClick.AddListener(OnGoldButtonClick);
        }

        // 초기 UI 상태 (게임 시작 시에는 모든 보상 패널이 꺼져있어야 함)
        if (RewardPanel != null) RewardPanel.SetActive(false);
        if (RuneRewardPanel != null) RuneRewardPanel.SetActive(false);
        if (mapButton != null) mapButton.gameObject.SetActive(false);
    }

    public void ShowRewardPanel()
    {
        hasClaimedRuneReward = false; // 새 보상 단계이므로 보상 수령 상태 초기화
        hasClaimedGoldReward = false;

        if (RewardPanel != null) RewardPanel.SetActive(true);
        if (RuneRewardPanel != null) RuneRewardPanel.SetActive(false); // 룬 선택 패널은 닫힌 상태로 시작

        // 룬/골드 보상 버튼 초기 상태 설정
        if (RewardRuneButton != null) RewardRuneButton.interactable = true;
        if (RewardGoldButton != null) RewardGoldButton.interactable = true;

        if (mapButton != null)
        {
            mapButton.gameObject.SetActive(false); // 맵 버튼은 보상 선택 전까지 비활성화
            Debug.Log("[RewardManager] ShowRewardPanel: MapButton 비활성화됨.");
        }
    }

    private void OpenRuneRewardPanel()
    {
        if (hasClaimedRuneReward) // 이미 룬 보상을 받았다면 다시 열지 않음
        {
            Debug.Log("[RewardManager] 이미 이번 전투의 룬 보상을 받았습니다.");
            return;
        }

        if (RewardPanel != null) RewardPanel.SetActive(false); // 메인 보상 패널은 닫거나 그대로 둘 수 있음 (디자인에 따라)

        if (RuneRewardPanel == null || OptionButtons == null || RewardPool == null || RewardPool.Count == 0)
        {
            Debug.LogError("[RewardManager] OpenRuneRewardPanel: 필요한 UI 요소 또는 보상 풀이 설정되지 않았습니다.");
            CheckAndActivateMapButton(); // 오류 시에도 다음으로 넘어갈 수 있게 처리
            return;
        }

        var availableRewardPool = new List<RuneSO>(RewardPool); // 실제 제시할 룬 풀
        var choices = new List<RuneSO>();
        int maxChoices = Mathf.Min(OptionButtons.Length, availableRewardPool.Count);

        for (int i = 0; i < maxChoices; i++)
        {
            if (availableRewardPool.Count == 0) break;
            int idx = Random.Range(0, availableRewardPool.Count);
            choices.Add(availableRewardPool[idx]);
            availableRewardPool.RemoveAt(idx); // 중복 방지
        }

        for (int i = 0; i < OptionButtons.Length; i++)
        {
            var button = OptionButtons[i];
            if (button == null) continue;

            if (i < choices.Count && choices[i] != null)
            {
                var so = choices[i]; // 클로저를 위해 로컬 변수 사용
                button.gameObject.SetActive(true);

                Image iconImage = button.GetComponent<Image>();
                if (iconImage != null) iconImage.sprite = so.icon;

                TMP_Text nameText = button.GetComponentInChildren<TMP_Text>();
                if (nameText != null) nameText.text = so.displayName;

                button.onClick.RemoveAllListeners();
                button.onClick.AddListener(() => {
                    OnRuneOptionChosen(so); // `so`는 여기서 캡처된 특정 RuneSO 인스턴스
                });
            }
            else
            {
                button.gameObject.SetActive(false);
            }
        }
        RuneRewardPanel.SetActive(true);
    }

    private void OnRuneOptionChosen(RuneSO chosenRune)
    {
        if (chosenRune == null)
        {
            Debug.LogError("[RewardManager] OnRuneOptionChosen: chosenRune이 null입니다.");
            return;
        }
        if (RuneDeckManager.Instance == null)
        {
            Debug.LogError("[RewardManager] OnRuneOptionChosen: RuneDeckManager.Instance가 null입니다.");
            CheckAndActivateMapButton(); // 다음으로 진행은 가능하게
            return;
        }

        Debug.Log($"[RewardManager] OnRuneOptionChosen 호출됨: {chosenRune.name}. 덱 변경 및 저장 시도.");
        RuneDeckManager.Instance.ReplaceBasicWithReward(chosenRune.name);
        RuneDeckManager.Instance.SaveDeckState();

        if (RuneRewardPanel != null) RuneRewardPanel.SetActive(false);
        // if (RewardPanel != null) RewardPanel.SetActive(true); // 메인 보상 패널을 다시 보여줄지 결정

        hasClaimedRuneReward = true;
        if (RewardRuneButton != null) RewardRuneButton.interactable = false; // 룬 보상 버튼 비활성화

        CheckAndActivateMapButton(); // 골드 보상도 받았는지 확인 후 맵 버튼 활성화
    }

    public void OnGoldButtonClick()
    {
        if (hasClaimedGoldReward) // 이미 골드 보상을 받았다면 다시 받지 않음
        {
            Debug.Log("[RewardManager] 이미 이번 전투의 골드 보상을 받았습니다.");
            return;
        }
        if (Player.Instance == null)
        {
            Debug.LogError("[RewardManager] OnGoldButtonClick: Player.Instance가 null입니다.");
            CheckAndActivateMapButton(); // 다음으로 진행은 가능하게
            return;
        }

        Player.Instance.AddGold(100);
        Debug.Log("[RewardManager] 골드 100 획득.");

        hasClaimedGoldReward = true;
        if (RewardGoldButton != null) RewardGoldButton.interactable = false; // 골드 버튼 비활성화

        CheckAndActivateMapButton(); // 룬 보상도 받았는지 확인 후 맵 버튼 활성화
    }

    /// <summary>
    /// 모든 필수 보상을 받았는지 확인하고 맵 버튼을 활성화하는 헬퍼 메서드.
    /// 현재는 룬 또는 골드 중 하나라도 받으면 (또는 둘 다 받으면) 맵 버튼이 활성화되도록 합니다.
    /// 만약 둘 다 받아야만 활성화되도록 하려면 조건 변경 필요.
    /// </summary>
    private void CheckAndActivateMapButton()
    {
        // 예시: 룬 보상과 골드 보상 버튼이 모두 비활성화(즉, 선택 완료) 상태일 때 맵 버튼 활성화
        bool runeRewardDone = (RewardRuneButton == null || !RewardRuneButton.interactable);
        bool goldRewardDone = (RewardGoldButton == null || !RewardGoldButton.interactable);

        // 또는 hasClaimedRuneReward와 hasClaimedGoldReward 플래그를 사용할 수 있습니다.
        // 여기서는 "둘 중 하나라도 보상을 선택했거나, 두 보상 버튼이 모두 비활성화(선택 불가) 상태가 되면" 다음으로 진행 가능하게 합니다.
        // 만약 "룬 보상과 골드 보상 둘 다 받아야 한다"면 (hasClaimedRuneReward && hasClaimedGoldReward) 조건을 사용합니다.
        // 현재 로직은 "룬 보상을 받거나 or 골드 보상을 받으면" 다음으로 진행 가능하게 하는 것으로 해석될 수 있습니다.
        // 기획에 따라 "둘 다 받아야만 다음으로" 또는 "하나만 받아도 다음으로 (나머지는 포기)" 등을 결정해야 합니다.

        // 여기서는 "플레이어가 더 이상 취할 보상 액션이 없다고 판단될 때" (즉, 남은 보상 버튼이 없거나 비활성화)
        // 또는 "하나라도 보상을 받았다면" 다음으로 넘어갈 수 있도록 mapButton을 활성화하는 로직을 가정합니다.
        // 좀 더 명확하게는, "룬 보상 버튼과 골드 보상 버튼이 모두 비활성화 상태이면" 맵 버튼을 활성화합니다.
        if (runeRewardDone && goldRewardDone)
        {
            if (mapButton != null)
            {
                mapButton.gameObject.SetActive(true);
                Debug.Log("[RewardManager] 모든 보상 선택 완료 (또는 선택 불가), MapButton 활성화됨.");
            }
            if (RewardPanel != null) RewardPanel.SetActive(false); // 모든 보상 선택 후 메인 패널 닫기
        }
        else if (mapButton != null && (hasClaimedRuneReward || hasClaimedGoldReward) && !mapButton.gameObject.activeSelf)
        {
            // 만약 룬 보상이나 골드 보상 중 하나만 받고 바로 다음으로 진행하고 싶다면 이 로직도 유효.
            // mapButton.gameObject.SetActive(true);
            // Debug.Log("[RewardManager] 일부 보상 선택 완료, MapButton 활성화됨.");
        }
    }
}