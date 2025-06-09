// RewardManager.cs
using System.Collections.Generic;
using System.Linq;
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

    private bool hasClaimedRuneReward = false;
    private bool hasClaimedGoldReward = false;

    public void OnRuneRewardChosen(string rewardRuneID)
    {
        Debug.LogWarning($"[RewardManager] OnRuneRewardChosen 호출됨 (룬 ID: {rewardRuneID}). " +
                         "이 메서드는 OnRuneOptionChosen과 기능이 중복될 수 있으므로 호출 경로 및 역할 확인 필요.");
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

        if (RewardPanel != null) RewardPanel.SetActive(false);
        if (RuneRewardPanel != null) RuneRewardPanel.SetActive(false);
        if (mapButton != null) mapButton.gameObject.SetActive(false);
    }

    public void ShowRewardPanel()
    {
        hasClaimedRuneReward = false;
        hasClaimedGoldReward = false;

        if (RewardPanel != null) RewardPanel.SetActive(true);
        if (RuneRewardPanel != null) RuneRewardPanel.SetActive(false);

        if (RewardRuneButton != null) RewardRuneButton.interactable = true;
        if (RewardGoldButton != null) RewardGoldButton.interactable = true;

        if (mapButton != null)
        {
            mapButton.gameObject.SetActive(false);
            Debug.Log("[RewardManager] ShowRewardPanel: MapButton 비활성화됨.");
        }
    }

    // ▼▼▼ 이 함수를 수정합니다 ▼▼▼
    private void OpenRuneRewardPanel()
    {
        if (hasClaimedRuneReward)
        {
            Debug.Log("[RewardManager] 이미 이번 전투의 룬 보상을 받았습니다.");
            return;
        }

        if (RewardPanel != null) RewardPanel.SetActive(false);

        if (RuneRewardPanel == null || OptionButtons == null || RewardPool == null || RewardPool.Count == 0)
        {
            Debug.LogError("[RewardManager] OpenRuneRewardPanel: 필요한 UI 요소 또는 보상 풀이 설정되지 않았습니다.");
            CheckAndActivateMapButton();
            return;
        }

        var availableRewardPool = new List<RuneSO>(RewardPool);
        var choices = new List<RuneSO>();
        int maxChoices = Mathf.Min(OptionButtons.Length, availableRewardPool.Count);

        for (int i = 0; i < maxChoices; i++)
        {
            if (availableRewardPool.Count == 0) break;
            int idx = Random.Range(0, availableRewardPool.Count);
            choices.Add(availableRewardPool[idx]);
            availableRewardPool.RemoveAt(idx);
        }

        for (int i = 0; i < OptionButtons.Length; i++)
        {
            var button = OptionButtons[i];
            if (button == null) continue;

            if (i < choices.Count && choices[i] != null)
            {
                var so = choices[i];
                button.gameObject.SetActive(true);

                Image iconImage = button.GetComponent<Image>();
                if (iconImage != null) iconImage.sprite = so.icon;

                TMP_Text nameText = button.GetComponentInChildren<TMP_Text>();
                if (nameText != null) nameText.text = so.displayName;

                // --- 툴팁 핸들러에 룬 데이터 설정 추가 ---
                RuneTooltipHandler handler = button.GetComponent<RuneTooltipHandler>();
                if (handler != null)
                {
                    handler.runeSO = so; // 이 버튼이 표시할 룬 정보를 핸들러에 전달
                }
                else
                {
                    // 핸들러가 없다면 동적으로 추가하고 경고를 남깁니다. (프리팹에 미리 추가해두는 것이 가장 좋습니다.)
                    Debug.LogWarning($"{button.name}에 RuneTooltipHandler가 없어 동적으로 추가했습니다.");
                    handler = button.gameObject.AddComponent<RuneTooltipHandler>();
                    handler.runeSO = so;
                }
                // --- 툴팁 핸들러 설정 완료 ---

                button.onClick.RemoveAllListeners();
                button.onClick.AddListener(() => {
                    OnRuneOptionChosen(so);
                });
            }
            else
            {
                button.gameObject.SetActive(false);
            }
        }
        RuneRewardPanel.SetActive(true);
    }
    // ▲▲▲ 함수 수정 완료 ▲▲▲

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
            CheckAndActivateMapButton();
            return;
        }

        Debug.Log($"[RewardManager] OnRuneOptionChosen 호출됨: {chosenRune.name}. 덱 변경 및 저장 시도.");
        RuneDeckManager.Instance.ReplaceBasicWithReward(chosenRune.name);
        RuneDeckManager.Instance.SaveDeckState();

        if (RuneRewardPanel != null) RuneRewardPanel.SetActive(false);
        if (RewardPanel != null) RewardPanel.SetActive(true);

        hasClaimedRuneReward = true;
        if (RewardRuneButton != null) RewardRuneButton.interactable = false;

        CheckAndActivateMapButton();
    }

    public void OnGoldButtonClick()
    {
        if (hasClaimedGoldReward)
        {
            Debug.Log("[RewardManager] 이미 이번 전투의 골드 보상을 받았습니다.");
            return;
        }
        if (Player.Instance == null)
        {
            Debug.LogError("[RewardManager] OnGoldButtonClick: Player.Instance가 null입니다.");
            CheckAndActivateMapButton();
            return;
        }

        Player.Instance.AddGold(100);
        Debug.Log("[RewardManager] 골드 100 획득.");

        hasClaimedGoldReward = true;
        if (RewardGoldButton != null) RewardGoldButton.interactable = false;

        CheckAndActivateMapButton();
    }

    private void CheckAndActivateMapButton()
    {
        bool runeRewardDone = (RewardRuneButton == null || !RewardRuneButton.interactable);
        bool goldRewardDone = (RewardGoldButton == null || !RewardGoldButton.interactable);

        if (runeRewardDone && goldRewardDone)
        {
            if (mapButton != null)
            {
                mapButton.gameObject.SetActive(true);
                Debug.Log("[RewardManager] 모든 보상 선택 완료 (또는 선택 불가), MapButton 활성화됨.");
            }
            if (RewardPanel != null) RewardPanel.SetActive(false);
        }
    }
}