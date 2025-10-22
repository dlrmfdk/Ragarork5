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


    [Header("룬 강화 UI")]
    [SerializeField] private GameObject EnhancementPanel;
    [SerializeField] private List<EnhancementSlotUI> enhancementSlots;

    [Header("보상 룬 SO 목록 (Inspector에서 채워주세요)")]
    [SerializeField] private List<RuneSO> RewardPool;

    private bool hasClaimedRuneReward = false;
    private bool hasClaimedGoldReward = false;

    private RuneSO selectedRewardRune; // 선택한 보상 룬을 임시 저장할 변수


    public void OnRuneRewardChosen(string rewardRuneID)
    {
        Debug.LogWarning($"[RewardManager] OnRuneRewardChosen 호출됨 (룬 ID: {rewardRuneID}). " +
                         "이 메서드는 OnRuneOptionChosen과 기능이 중복될 수 있으므로 호출 경로 및 역할 확인 필요.");
    }

    private void Awake()
    {
        // 1. 싱글톤 패턴(Singleton Pattern) 설정
        if (Instance == null)
        {
            Instance = this; // 처음 생성된 RewardManager라면, 자신을 '대장'으로 임명
        }
        else
        {
            Destroy(gameObject); // 이미 '대장'이 있다면, 새로 생긴 자신은 파괴
            return;
        }

        // 2. 보상 버튼에 기능 연결
        if (RewardRuneButton != null)
        {
            RewardRuneButton.onClick.RemoveAllListeners(); // 기존에 설정된 클릭 이벤트를 모두 제거
            RewardRuneButton.onClick.AddListener(OpenRuneRewardPanel); // '룬 보상' 버튼을 누르면 OpenRuneRewardPanel 함수가 실행되도록 연결
        }
        if (RewardGoldButton != null)
        {
            RewardGoldButton.onClick.RemoveAllListeners();
            RewardGoldButton.onClick.AddListener(OnGoldButtonClick); // '골드 보상' 버튼 기능 연결
        }

        // 3. UI 패널 초기 상태 설정
        if (RewardPanel != null) RewardPanel.SetActive(false);       // 메인 보상 패널 숨기기
        if (RuneRewardPanel != null) RuneRewardPanel.SetActive(false); // 룬 선택 패널 숨기기
        if (mapButton != null) mapButton.gameObject.SetActive(false);   // 다음 맵으로 가는 버튼 숨기기
    }

    public void ShowRewardPanel() //보상 창 활성화
    {
        // 1. 보상 획득 상태 초기화
        hasClaimedRuneReward = false;
        hasClaimedGoldReward = false;

        // 2. 보여줄 패널과 숨길 패널 설정
        if (RewardPanel != null) RewardPanel.SetActive(true);       // '룬/골드'를 선택하는 메인 보상 패널을 보여줌
        if (RuneRewardPanel != null) RuneRewardPanel.SetActive(false); // 룬 3개 중 하나를 고르는 패널은 아직 숨김

        // 3. 버튼 활성화
        if (RewardRuneButton != null) RewardRuneButton.interactable = true; // '룬 보상' 버튼을 클릭할 수 있게 만듦
        if (RewardGoldButton != null) RewardGoldButton.interactable = false; // 골드 보상' 버튼은 우선 비활성화
                                                                           

        // 4. 다음으로 진행하는 버튼 비활성화
        if (mapButton != null)
        {
            mapButton.gameObject.SetActive(false); // 보상을 모두 받기 전에는 다음으로 진행 불가
            Debug.Log("[RewardManager] ShowRewardPanel: MapButton 비활성화됨.");
        }
    }

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



    private void OnRuneOptionChosen(RuneSO chosenRewardRune)
    {
        UIManager.Instance.HideRuneTooltip();

        this.selectedRewardRune = chosenRewardRune; // 1. 선택한 보상 룬을 기억합니다.
        RuneRewardPanel?.SetActive(false); // 보상 선택창은 닫습니다.

        // 2. 덱에서 강화할 수 있는 같은 색의 기본 룬 목록을 가져옵니다.
        //var basicRunes = RuneDeckManager.Instance.GetBasicRunesByColor(chosenRewardRune.color);
        // ▼▼▼ [수정] 'GetBasicRunesByColor' -> 'GetAllRunesByColor'로 변경 ▼▼▼
        // 2. 덱에서 강화할 수 있는 같은 색의 '모든 룬' 목록을 가져옵니다.
        var runesToEnhance = RuneDeckManager.Instance.GetAllRunesByColor(chosenRewardRune.color);
        // ▲▲▲ 수정 완료 (변수 이름도 basicRunes -> runesToEnhance로 변경) ▲▲▲

        if (runesToEnhance.Count > 0)
        {
            // 3. 강화할 룬이 있다면 강화 패널을 엽니다.
            ShowEnhancementPanel(runesToEnhance);
        }
        else
        {
            // 강화할 룬이 없으면 보상 획득 실패 처리
            Debug.LogWarning($"강화할 {chosenRewardRune.color}색 기본 룬이 없어 보상을 받을 수 없습니다.");
            FinishRuneReward();
        }
    }
    private void ShowEnhancementPanel(List<RuneInstance> basicRunes)
    {
        // Inspector에 연결된 슬롯들을 사용합니다.
        for (int i = 0; i < enhancementSlots.Count; i++)
        {
            if (i < basicRunes.Count)
            {
                // 각 슬롯 UI에 룬 인스턴스 정보와, 클릭 시 실행될 콜백 함수를 전달합니다.
                enhancementSlots[i].Setup(basicRunes[i], OnBasicRuneSelectedForEnhancement);
            }
            else
            {
                // 덱에 있는 기본 룬 개수만큼만 슬롯을 보여주고 나머지는 숨깁니다.
                enhancementSlots[i].gameObject.SetActive(false);
            }
        }
        EnhancementPanel?.SetActive(true);
    }

    // 플레이어가 강화할 기본 룬을 최종 선택했을 때 호출될 함수
    private void OnBasicRuneSelectedForEnhancement(RuneInstance chosenBasicRune)
    {
        // RuneDeckManager에 최종 강화 명령을 내립니다.
        RuneDeckManager.Instance.EnhanceRune(chosenBasicRune, selectedRewardRune);

        EnhancementPanel?.SetActive(false); // 강화 패널을 닫습니다.
        FinishRuneReward(); // 보상 절차를 마무리합니다.
    }
    private void FinishRuneReward()
    {
        // 1. 이번 전투에서 룬 보상을 받았다고 기록합니다.
        hasClaimedRuneReward = true;

        // 2. 메인 보상 화면의 '룬 보상' 버튼을 비활성화하여 다시 누를 수 없게 만듭니다.
        if (RewardRuneButton != null)
        {
            RewardRuneButton.interactable = false;
        }
        // ▼▼▼ 이 코드를 추가하세요 ▼▼▼
        // 3. 룬 보상이 끝났으므로 이제 골드 보상 버튼을 활성화합니다.
        if (RewardGoldButton != null && !hasClaimedGoldReward) // 아직 골드 보상을 안받았다면
        {
            RewardGoldButton.interactable = true;
        }
        // ▲▲▲ 추가 완료 ▲▲▲

        // 4. 골드 보상도 받았는지 함께 확인하여, 모든 보상을 다 받았다면 
        //    다음 스테이지로 가는 버튼을 활성화하는 함수를 호출합니다.
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
            // 1. 룬과 골드 보상을 '모두' 받았다면,
            Debug.Log("[RewardManager] 모든 보상을 획득했습니다. 다음으로 진행합니다.");
            if (mapButton != null) mapButton.gameObject.SetActive(true); // 다음 버튼 활성화
            if (RewardPanel != null) RewardPanel.SetActive(false);     // 메인 보상 패널 숨기기
        }
        else
        {
            // 2. 아직 받지 않은 보상이 '남아있다면',
            Debug.Log("[RewardManager] 아직 받지 않은 보상이 있습니다. 메인 보상 패널로 돌아갑니다.");
            if (RewardPanel != null) RewardPanel.SetActive(true); // 메인 보상 패널을 다시 보여줌
        }
    }
}