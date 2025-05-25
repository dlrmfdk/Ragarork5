using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class UIManager : MonoBehaviour
{
    public static UIManager Instance { get; private set; }
    public static event Action OnUIManagerReady; // UIManager가 준비되었음을 알리는 static 이벤트

    void Awake()
    {
        Debug.Log($"[UIManager.Awake] UIManager 게임 오브젝트 '{this.gameObject.name}'의 Awake 호출됨. Instance 설정 시도. 현재 UIManager.Instance는 {(Instance == null ? "null" : Instance.gameObject.name)}");
        if (Instance != null && Instance != this)
        {
            Debug.LogWarning($"[UIManager.Awake] 다른 UIManager 인스턴스('{Instance.gameObject.name}')가 이미 존재하여 현재 인스턴스('{this.gameObject.name}')를 파괴합니다.");
            Destroy(gameObject);
            return;
        }
        Instance = this;
        // UIManager는 씬에 종속적이므로 DontDestroyOnLoad를 사용하지 않습니다.
        Debug.Log($"[UIManager.Awake] UIManager.Instance가 '{Instance.gameObject.name}'으로 설정됨.");
    }

    void Start()
    {
        // 초기 UI 상태 설정
        runeDeckPanel.SetActive(false);
        centralSlotPanel.SetActive(false);
        if (drawButton != null) drawButton.interactable = false; // null 체크 추가
        if (rerollButton != null) rerollButton.interactable = false; // null 체크 추가

        Debug.Log($"[UIManager.Start] UIManager '{this.gameObject.name}' Start() 호출됨. OnUIManagerReady 이벤트 발생 시도. 현재 Instance: {(Instance == null ? "null" : Instance.gameObject.name)}");
        // UIManager의 모든 설정이 완료된 후 이벤트 발생
        OnUIManagerReady?.Invoke();
    }

    void OnDestroy()
    {
        Debug.Log($"[UIManager.OnDestroy] UIManager 게임 오브젝트 '{this.gameObject.name}'의 OnDestroy 호출됨.");
        if (Instance == this)
        {
            Instance = null;
            Debug.Log("[UIManager.OnDestroy] 현재 Instance가 파괴되는 인스턴스와 동일하여 UIManager.Instance를 null로 설정함.");
        }
        // 버튼 리스너는 RuneDeckManager가 BindRuneDeck(null, null) 등으로 해제하거나,
        // UIManager가 자신의 버튼들을 관리한다면 여기서 정리할 수 있지만,
        // 현재 BindRuneDeck에서 RemoveAllListeners()를 하므로 필수적이지 않을 수 있습니다.
    }

    [Header("룬 덱 버튼들")]
    public Button redButton;
    public TextMeshProUGUI redCountText;
    public Button blueButton;
    public TextMeshProUGUI blueCountText;
    public Button whiteButton;
    public TextMeshProUGUI whiteCountText;
    public Button yellowButton;
    public TextMeshProUGUI yellowCountText;

    [Header("중앙 슬롯")]
    public List<Button> slotButtons;      // 5개 슬롯 버튼
    public List<Image> slotIconImages;   // 슬롯 아이콘 표시용

    [Header("뽑기/리롤 버튼")]
    public Button drawButton;
    public Button rerollButton;

    [Header("패널들")]
    public GameObject runeDeckPanel;
    public GameObject centralSlotPanel;

    public void BindRuneDeck(Action<RuneColor> onDeckClick, Action onDrawClick)
    {
        // 각 버튼이 null이 아닌지 확인 후 리스너 설정
        if (redButton != null)
        {
            redButton.onClick.RemoveAllListeners();
            if (onDeckClick != null) redButton.onClick.AddListener(() => onDeckClick(RuneColor.Red));
        }
        if (blueButton != null)
        {
            blueButton.onClick.RemoveAllListeners();
            if (onDeckClick != null) blueButton.onClick.AddListener(() => onDeckClick(RuneColor.Blue));
        }
        if (whiteButton != null)
        {
            whiteButton.onClick.RemoveAllListeners();
            if (onDeckClick != null) whiteButton.onClick.AddListener(() => onDeckClick(RuneColor.White));
        }
        if (yellowButton != null)
        {
            yellowButton.onClick.RemoveAllListeners();
            if (onDeckClick != null) yellowButton.onClick.AddListener(() => onDeckClick(RuneColor.Yellow));
        }

        if (drawButton != null)
        {
            drawButton.onClick.RemoveAllListeners();
            if (onDrawClick != null) drawButton.onClick.AddListener(() => onDrawClick());
        }
    }

    public void BindReRoll(Action onReRoll)
    {
        if (rerollButton != null)
        {
            rerollButton.onClick.RemoveAllListeners();
            if (onReRoll != null) rerollButton.onClick.AddListener(() => onReRoll());
        }
    }

    public void SetReRollButton(bool enabled)
    {
        if (rerollButton != null) rerollButton.interactable = enabled;
    }

    public void SetDrawButton(bool enabled)
    {
        if (drawButton != null) drawButton.interactable = enabled;
    }

    public void ShowRuneUI()
    {
        if (runeDeckPanel != null) runeDeckPanel.SetActive(true);
        if (centralSlotPanel != null) centralSlotPanel.SetActive(true);
    }

    public void HideRuneUI()
    {
        if (runeDeckPanel != null) runeDeckPanel.SetActive(false);
        if (centralSlotPanel != null) centralSlotPanel.SetActive(false);
    }

    public void UpdateDeckCounts(Dictionary<RuneColor, int> counts)
    {
        if (redCountText != null && counts.ContainsKey(RuneColor.Red)) redCountText.text = counts[RuneColor.Red].ToString();
        if (blueCountText != null && counts.ContainsKey(RuneColor.Blue)) blueCountText.text = counts[RuneColor.Blue].ToString();
        if (whiteCountText != null && counts.ContainsKey(RuneColor.White)) whiteCountText.text = counts[RuneColor.White].ToString();
        if (yellowCountText != null && counts.ContainsKey(RuneColor.Yellow)) yellowCountText.text = counts[RuneColor.Yellow].ToString();
    }

    public void UpdateCentralSlotsWithSO(List<RuneSO> selections)
    {
        Debug.Log($"[UIManager.UpdateCentralSlotsWithSO] 슬롯 아이콘 업데이트 시작. selections 개수: {(selections == null ? "null" : selections.Count.ToString())}");
        if (slotIconImages == null) return;

        for (int i = 0; i < slotIconImages.Count; i++)
        {
            if (slotIconImages[i] == null) continue;

            if (selections != null && i < selections.Count && selections[i] != null)
            {
                var so = selections[i];
                Debug.Log($"Slot {i}: 룬 {so.name}, 아이콘 {(so.icon == null ? "NULL" : so.icon.name)}");
                slotIconImages[i].sprite = so.icon;
                slotIconImages[i].enabled = true;
            }
            else
            {
                slotIconImages[i].enabled = false;
            }
        }
    }
}