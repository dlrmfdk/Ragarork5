using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class UIManager : MonoBehaviour
{
    public static UIManager Instance { get; private set; }
    public static event Action OnUIManagerReady; // UIManager가 준비되었음을 알리는 static 이벤트

    [Header("툴팁 설정")]
    [Tooltip("기준 UI(룬 버튼) 위치에서 툴팁이 얼마나 떨어져 표시될지 설정합니다.")]
    public Vector2 tooltipOffset = new Vector2(0, 80); // Y값을 조절하여 버튼 위/아래 간격 설정

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

        if (runeDeckPanel != null)
        {
            runeDeckPanel.SetActive(false);
        }


        if (centralSlotPanel != null)
        {
            centralSlotPanel.SetActive(false);
        }


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
    public List<RuneSlotUI> centralRuneSlots;

    [Header("뽑기/리롤 버튼")]
    public Button drawButton;
    public Button rerollButton;

    [Header("패널들")]
    public GameObject runeDeckPanel;
    public GameObject centralSlotPanel;
    public GameObject runeTooltipPanel; // 1번에서 만든 RuneTooltipPanel 오브젝트를 할당
    public TextMeshProUGUI tooltipText; // 1번에서 만든 TooltipText 오브젝트의 TextMeshProUGUI 컴포넌트를 할당


    [Header("플레이어 상태 UI")]
    public TextMeshProUGUI goldText; // Player.cs 대신 UIManager가 직접 참조


    /// <summary>
    /// 골드 표시 UI를 업데이트합니다. Player 등 외부에서 호출됩니다.
    /// </summary>
    /// <param name="amount">표시할 골드의 양</param>
    public void UpdateGoldDisplay(int amount)
    {
        if (goldText != null)
        {
            goldText.text = amount.ToString();
        }
        else
        {
            // 이 경고는 골드 UI가 없는 씬(예: 메인 메뉴)에서 정상적으로 나타날 수 있습니다.
            Debug.LogWarning("[UIManager] Gold Text UI가 할당되지 않았습니다.");
        }
    }


    /// <summary>
    /// 룬 설명 툴팁을 보여줍니다.
    /// </summary>
    /// <param name="runeSO">표시할 룬의 데이터</param>
    /// <param name="anchorTransform">툴팁 위치의 기준이 될 UI의 RectTransform</param>
    // UIManager.cs의 ShowRuneTooltip 메서드 수정

    /// <summary>
    /// 룬 인스턴스 정보를 받아 동적인 설명으로 툴팁을 보여줍니다.
    /// </summary>



    /// <summary>
    /// (보상 화면용) RuneSO를 받아 정적인 설명으로 툴팁을 보여줍니다.
    /// </summary>
    public void ShowRuneTooltip(RuneSO runeSO, RectTransform anchorTransform)
    {
        if (runeTooltipPanel == null || tooltipText == null || runeSO == null || anchorTransform == null) return;

        // "n"을 바꾸지 않고 SO의 설명 그대로 표시합니다.
        tooltipText.text = $"<b>{runeSO.displayName}</b>\n\n{runeSO.description}";

        runeTooltipPanel.transform.position = (Vector2)anchorTransform.position + tooltipOffset;
        runeTooltipPanel.SetActive(true);
    }

    /// <summary>
    /// (핸드 슬롯용) RuneInstance를 받아 동적인 설명으로 툴팁을 보여줍니다.
    /// </summary>
    public void ShowRuneTooltip(RuneInstance runeInstance)
    {
        if (runeTooltipPanel == null || tooltipText == null || runeInstance?.SO == null) return;

        string formattedDesc = runeInstance.SO.description.Replace("n", runeInstance.value.ToString());
        tooltipText.text = $"<b>{runeInstance.SO.displayName}</b>\n\n{formattedDesc}";

        runeTooltipPanel.transform.position = Input.mousePosition + (Vector3)tooltipOffset;
        runeTooltipPanel.SetActive(true);
    }

    /// <summary>
    /// 룬 설명 툴팁을 숨깁니다.
    /// </summary>
    public void HideRuneTooltip()
    {
        if (runeTooltipPanel == null) return;
        runeTooltipPanel.SetActive(false);
    }


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

    // UIManager.cs

    // UIManager.cs

    public void UpdateCentralSlotsWithInstances(List<RuneInstance> selectedInstances)
    {
        // centralRuneSlots 리스트는 RuneTooltipHandler 타입의 리스트여야 합니다.
        for (int i = 0; i < centralRuneSlots.Count; i++)
        {
            if (i < selectedInstances.Count && selectedInstances[i] != null)
            {
                // 각 슬롯의 핸들러에 RuneInstance 정보를 전달하여 스스로 설정하도록 합니다.
                centralRuneSlots[i].Setup(selectedInstances[i]);
                centralRuneSlots[i].gameObject.SetActive(true);
            }
            else
            {
                // 빈 슬롯은 비활성화합니다.
                centralRuneSlots[i].gameObject.SetActive(false);
            }
        }
    
}


     
}
    