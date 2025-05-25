using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class RewardManager : MonoBehaviour
{
    public static RewardManager Instance { get; private set; }


    [Header("메인 보상 패널")]
    [SerializeField] private GameObject RewardPanel;        // 메인 보상 패널
    [SerializeField] private Button RewardRuneButton;   // ‘룬 보상’ 버튼
    [SerializeField] private Button RewardGoldButton;   // ‘골드 보상’ 버튼

    [Header("다음 진행 버튼")]
    [SerializeField] private Button mapButton; // Inspector에서 MapButton 연결

    [Header("룬 보상 UI")]
    [SerializeField] private GameObject RuneRewardPanel;    // 룬 옵션 서브 패널
    [SerializeField] private Button[] OptionButtons;      // 옵션 버튼 1~3

    [Header("보상 룬 SO 목록 (Inspector에서 채워주세요)")]
    [SerializeField] private List<RuneSO> RewardPool;       // 보상으로 제시할 RuneSO 리스트

    // 에디터에서 연결
    [SerializeField] private RuneDeckManager deckManager;

    //보상 룬 선택했을때
    public void OnRuneRewardChosen(string rewardRuneID)
    {
       // 1) 보상룬 ID로 덱 매니저에 교체 요청
        RuneDeckManager.Instance.ReplaceBasicWithReward(rewardRuneID);

      // 2) 교체된 덱 상태를 즉시 저장
       RuneDeckManager.Instance.SaveDeckState();

        // 3) 보상 UI 닫기
        RuneRewardPanel.SetActive(false);
    }
private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;

            // 초기 UI 상태
            RewardPanel.SetActive(false);
            RuneRewardPanel.SetActive(false);

            // 메인 패널 ‘룬 보상’ 버튼에 클릭 리스너 등록
            RewardRuneButton.onClick.RemoveAllListeners();
            RewardRuneButton.onClick.AddListener(OpenRuneRewardPanel);

          
            RewardGoldButton.onClick.RemoveAllListeners();
            RewardGoldButton.onClick.AddListener(OnGoldButtonClick);

        }
        else
        {
            Destroy(gameObject);
        }
    }

    /// <summary>
    /// 전투 종료 직후 호출: 메인 보상 패널만 켜기
    /// </summary>
    public void ShowRewardPanel()
    {
        RewardPanel.SetActive(true);
        RuneRewardPanel.SetActive(false);

        if (mapButton != null)
        {
            mapButton.gameObject.SetActive(false); // 또는 mapButton.interactable = false;
            Debug.Log("[RewardManager] MapButton 비활성화됨.");
        }
    }

    /// <summary>
    /// ‘룬 보상’ 버튼 클릭 시 열리는 패널
    /// </summary>
    private void OpenRuneRewardPanel()
    {
        // 1) 메인 패널 닫기
        RewardPanel.SetActive(false);

        // 2) 보상풀 복사 및 랜덤 3개 추출
        var tempPool = new List<RuneSO>(RewardPool);
        var choices = new List<RuneSO>();
        int maxCount = Mathf.Min(OptionButtons.Length, tempPool.Count);

        for (int i = 0; i < maxCount; i++)
        {
            int idx = Random.Range(0, tempPool.Count);
            choices.Add(tempPool[idx]);
            tempPool.RemoveAt(idx);
        }

        // 3) 옵션 버튼마다 정보 세팅
        for (int i = 0; i < OptionButtons.Length; i++)
        {
            var button = OptionButtons[i];

            if (i < choices.Count)
            {
                var so = choices[i];

                // 아이콘 설정
                var iconImage = button.GetComponent<Image>();
                iconImage.sprite = so.icon;

                // 이름 설정
                var nameText = button.GetComponentInChildren<TMP_Text>();
                nameText.text = so.displayName;

                // 클릭 리스너 등록
                button.gameObject.SetActive(true);
                button.onClick.RemoveAllListeners();
                button.onClick.AddListener(() => {
                    OnRuneOptionChosen(so);
                    OnRuneRewardChosen(so.name);
                });

            }
            else
            {
                // 선택지보다 버튼이 많으면 숨김
                button.gameObject.SetActive(false);
            }
        }

        // 4) 룬 보상 서브 패널 열기
        RuneRewardPanel.SetActive(true);
    }

    /// <summary>
    /// 보상 룬 하나 선택 시
    /// </summary>
    private void OnRuneOptionChosen(RuneSO chosenRune)
    {
        // 예: chosenRune.name 대신 string ID, 또는 직접 RuneSO 넘기기
        RuneDeckManager.Instance.ReplaceBasicWithReward(chosenRune.name);
        RuneDeckManager.Instance.SaveDeckState();
        // 보상 UI 닫기
        RuneRewardPanel.SetActive(false);
        if (mapButton != null)
        {
            mapButton.gameObject.SetActive(true); // 또는 mapButton.interactable = true;
            Debug.Log("[RewardManager] 보상 선택 완료, MapButton 활성화됨.");
        }
        
    }

    public void OnGoldButtonClick()
    {
    
        Player.Instance.AddGold(100); 

        
        RewardGoldButton.interactable = false;


    }
}
