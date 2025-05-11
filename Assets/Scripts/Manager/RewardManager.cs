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

    [Header("룬 보상 UI")]
    [SerializeField] private GameObject RuneRewardPanel;    // 룬 옵션 서브 패널
    [SerializeField] private Button[] OptionButtons;      // 옵션 버튼 1~3

    [Header("보상 룬 SO 목록 (Inspector에서 채워주세요)")]
    [SerializeField] private List<RuneSO> RewardPool;       // 보상으로 제시할 RuneSO 리스트

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

            // 메인 패널 ‘골드 보상’ 버튼에 클릭 리스너 등록
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
    }

    /// <summary>
    /// ‘룬 보상’ 버튼 클릭 시
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
                button.onClick.AddListener(() => OnRuneOptionChosen(so));
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
        // 덱에서 기본 룬 제거 후 보상 룬 추가
        RuneDeckManager.Instance.ReplaceBasicRune(chosenRune);

        // 보상 UI 닫기
        RuneRewardPanel.SetActive(false);
    }

    //골드 버튼 클릭 시 플레이어의 골드 증가 함수
    public void OnGoldButtonClick()
    {
        // 플레이어의 골드 증가
        Player.Instance.AddGold(100); // 예시로 100골드 증가

        //골드 보상 버튼 비활성화
        RewardGoldButton.interactable = false;


    }
}
