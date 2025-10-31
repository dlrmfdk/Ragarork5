using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.SceneManagement;
using System.Text.RegularExpressions; // ▼▼▼ Regex 사용 위해 추가 ▼▼▼
#if UNITY_EDITOR // 에디터 종료 위해 추가
using UnityEditor;
#endif

public class UIManager : MonoBehaviour
{
    public static UIManager Instance { get; private set; }
    public static event Action OnUIManagerReady; // UIManager가 준비되었음을 알리는 static 이벤트

    [Header("툴팁 설정")]
    [Tooltip("기준 UI(룬 버튼) 위치에서 툴팁이 얼마나 떨어져 표시될지 설정합니다.")]
    public Vector2 tooltipOffset = new Vector2(0, 80); // Y값을 조절하여 버튼 위/아래 간격 설정

    // ▼▼▼ 1. 게임 종료 버튼 변수 추가 ▼▼▼
    [Header("게임 오버 UI")]
    [SerializeField] private GameObject gameOverPanel;
    [SerializeField] private Button restartButton;
    [SerializeField] private Button exitButton; // 게임 종료 버튼 추가
    // ▲▲▲ 변수 추가 완료 ▲▲▲

    private RectTransform tooltipRect;
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

        // 시작할 때 툴팁 RectTransform을 미리 가져오기
        if (runeTooltipPanel != null)
        {
            tooltipRect = runeTooltipPanel.GetComponent<RectTransform>();
        }
        // UIManager는 씬에 종속적이므로 DontDestroyOnLoad를 사용하지 않습니다.
        Debug.Log($"[UIManager.Awake] UIManager.Instance가 '{Instance.gameObject.name}'으로 설정됨.");

        // 재시작 버튼에 RestartGame 함수 연결
        if (restartButton != null)
        {
            restartButton.onClick.RemoveAllListeners(); // 기존 리스너 제거
            restartButton.onClick.AddListener(RestartGame); // 함수 연결
        }

        // ▼▼▼ 2. 게임 종료 버튼 연결 코드 추가 ▼▼▼
        if (exitButton != null)
        {
            exitButton.onClick.RemoveAllListeners(); // 기존 리스너 제거
            exitButton.onClick.AddListener(OnClickExitGame); // 함수 연결
        }
        // ▲▲▲ 연결 코드 추가 완료 ▲▲▲

    }

    // ▼▼▼ 3. 게임 종료 함수 추가 (Lobby/PauseManager와 동일한 내용) ▼▼▼
    /// <summary>
    /// 게임 종료 버튼 클릭 시 호출될 함수입니다.
    /// </summary>
    public void OnClickExitGame()
    {
        Debug.Log("게임 종료 버튼 클릭됨 (게임 오버 화면)");
        Time.timeScale = 1f; // 안전하게 타임스케일 복구

#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#else
        Application.Quit();
#endif
    }
    // ▲▲▲ 함수 추가 완료 ▲▲▲
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

        //시작할 때 툴팁 숨기기
        HideRuneTooltip();

        Debug.Log($"[UIManager.Start] UIManager '{this.gameObject.name}' Start() 호출됨. OnUIManagerReady 이벤트 발생 시도. 현재 Instance: {(Instance == null ? "null" : Instance.gameObject.name)}");
        // UIManager의 모든 설정이 완료된 후 이벤트 발생
        OnUIManagerReady?.Invoke();
    }

    /// <summary>
    /// 게임 오버 패널을 화면에 표시합니다. Player.cs에서 호출됩니다.
    /// </summary>
    public void ShowGameOverPanel()
    {
        if (gameOverPanel != null)
        {
            gameOverPanel.SetActive(true);
            // 필요하다면 다른 UI들(룬 UI 등)을 여기서 숨길 수 있습니다.
            HideRuneUI();
        }
    }

    // ▼▼▼ 4. 게임 재시작 함수 추가 ▼▼▼
    /// <summary>
    /// 재시작 버튼 클릭 시 호출될 함수입니다.
    /// </summary>
    public void RestartGame()
    {
        // 게임 시간을 다시 정상으로 돌려놓습니다 (일시정지 상태였다면).
        Time.timeScale = 1f;

        // 게임의 가장 첫 씬(예: "TitleScene" 또는 "LobbyScene")을 로드합니다.
        // 만약 GameManager의 StartNewGame이 초기화 로직을 포함한다면 그것을 호출할 수도 있습니다.
        SceneManager.LoadScene("Lobby"); // 여기에 실제 타이틀 씬 이름을 넣으세요.
    }
    // ▲▲▲ 함수 추가 완료 ▲▲▲

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

    [Header("룬 예측 UI")]
    [Tooltip("손패의 총합 데미지를 표시할 텍스트")]
    [SerializeField] private TextMeshProUGUI previewDamageText;
    [Tooltip("손패의 총합 방어도를 표시할 텍스트")]
    [SerializeField] private TextMeshProUGUI previewDefenseText;
    [Tooltip("손패의 총합 골드 획득량을 표시할 텍스트")]
    [SerializeField] private TextMeshProUGUI previewGoldText;

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
    /// 손패에 든 룬의 단순 합계 값을 UI에 표시합니다.
    /// </summary>
    public void UpdatePreviewTotals(int damage, int defense, int gold)
    {
        // 데미지 텍스트
        if (previewDamageText != null)
        {
            if (damage > 0)
            {
                // 숫자를 문자열로 변환하여 텍스트에 표시
                previewDamageText.text = damage.ToString();
                previewDamageText.gameObject.SetActive(true);
            }
            else
            {
                previewDamageText.gameObject.SetActive(false); // 0이면 숨김
            }
        }

        // 방어도 텍스트
        if (previewDefenseText != null)
        {
            if (defense > 0)
            {
                previewDefenseText.text = defense.ToString();
                previewDefenseText.gameObject.SetActive(true);
            }
            else
            {
                previewDefenseText.gameObject.SetActive(false); // 0이면 숨김
            }
        }

        // 골드 텍스트
        if (previewGoldText != null)
        {
            if (gold > 0)
            {
                previewGoldText.text = gold.ToString();
                previewGoldText.gameObject.SetActive(true);
            }
            else
            {
                previewGoldText.gameObject.SetActive(false); // 0이면 숨김
            }
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
    /// (상태이상 아이콘 등) 단순한 제목과 설명 텍스트로 툴팁을 보여줍니다.
    /// </summary>
    public void ShowSimpleTooltip(string title, string description)
    {
        // 툴팁 UI가 연결되어 있는지 확인
        if (tooltipRect == null || tooltipText == null) return;

        // 1. 툴팁 내용 설정 (RuneSO나 RuneInstance 없이 텍스트만 사용)
        if (string.IsNullOrEmpty(title))
        {
            tooltipText.text = description; // 제목이 없으면 설명만
        }
        else
        {
            tooltipText.text = $"<b>{title}</b>\n\n{description}"; // 제목 + 설명
        }

        // ▼▼▼ 6. 이 코드를 추가하세요 ▼▼▼
        // 툴팁을 Hierarchy의 맨 아래로 보내 맨 위에 그리도록 함
        tooltipRect.transform.SetAsLastSibling();
        // ▲▲▲ 추가 완료 ▲▲▲
        // 2. 툴팁 활성화
        runeTooltipPanel.SetActive(true);

        // 3. 마우스 위치를 기준으로 오프셋을 더한 위치 계산 (RuneInstance 버전과 동일)
        Vector3 desiredPosition = Input.mousePosition + (Vector3)tooltipOffset;

        // 4. 위치 보정 및 최종 위치 설정
        tooltipRect.position = GetCorrectedTooltipPosition(desiredPosition);
    }
    // ▲▲▲ 함수 추가 완료 ▲▲▲


    /// <summary>
    /// (보상 화면용) RuneSO를 받아 정적인 설명으로 툴팁을 보여줍니다.
    /// </summary>
    public void ShowRuneTooltip(RuneSO runeSO, RectTransform anchorTransform)
    {
        if (tooltipRect == null || tooltipText == null || runeSO == null || anchorTransform == null) return;

        // 1. 툴팁 내용 설정
        tooltipText.text = $"<b>{runeSO.displayName}</b>\n\n{runeSO.description}";

        // 2. 툴팁 활성화 (크기 계산을 위해 먼저 활성화)
        runeTooltipPanel.SetActive(true);

        // 3. 앵커를 기준으로 이상적인 위치 계산
        //Vector3 desiredPosition = anchorTransform.position;
        // ▼▼▼ 3. 이 부분을 수정/추가하세요 ▼▼▼
        // 3. 앵커(버튼) 위치를 기준으로 '오프셋'을 더한 위치를 계산합니다.
        Vector3 desiredPosition = anchorTransform.position + (Vector3)tooltipOffset;
        // ▲▲▲ 수정 완료 ▲▲▲
        // 필요하다면 오프셋을 여기에 더할 수 있습니다.
        // desiredPosition += (Vector3)tooltipOffset;

        // 4. 위치 보정 및 최종 위치 설정
        tooltipRect.position = GetCorrectedTooltipPosition(desiredPosition);
    }

    /// <summary>
    /// (핸드 슬롯용) RuneInstance를 받아 동적인 설명으로 툴팁을 보여줍니다.
    /// </summary>
    /// 


    //public void ShowRuneTooltip(RuneInstance runeInstance)
    //{
    //    if (tooltipRect == null || tooltipText == null || runeInstance?.SO == null) return;

    //    // 1. 툴팁 내용 설정
    //    string formattedDesc = runeInstance.SO.description.Replace("n", runeInstance.value.ToString());
    //    tooltipText.text = $"<b>{runeInstance.SO.displayName}</b>\n\n{formattedDesc}";

    //    // 2. 툴팁 활성화
    //    runeTooltipPanel.SetActive(true);

    //    // 3. 마우스 위치를 기준으로 이상적인 위치 계산
    //    Vector3 desiredPosition = Input.mousePosition + (Vector3)tooltipOffset;

    //    // 4. 위치 보정 및 최종 위치 설정
    //    tooltipRect.position = GetCorrectedTooltipPosition(desiredPosition);
    //}



    public void ShowRuneTooltip(RuneInstance runeInstance)
    {
        if (tooltipRect == null || tooltipText == null || runeInstance?.SO == null) return;

        // 1. 원본 설명 가져오기
        string description = runeInstance.SO.description;
        int nValue = runeInstance.value; // 현재 룬의 값

        // ▼▼▼ 2. 골드 값 가져오기 (계산을 위해 미리) ▼▼▼
        int playerGold = 0;
        if (Player.Instance != null)
        {
            playerGold = Player.Instance.Gold;
        }
        else
        {
            Debug.LogWarning("Player.Instance를 찾을 수 없어 골드 관련 계산이 0이 될 수 있습니다.");
        }
        // ▲▲▲
        // 3.1. [n+(gold/DIVISOR)] 패턴 (예: [n+(gold/100)])
        description = Regex.Replace(description, @"\[n\+\(gold\/(\d+)\)\]", match =>
        {
            if (int.TryParse(match.Groups[1].Value, out int divisor) && divisor != 0)
            {
                int bonusDamage = playerGold / divisor; // 정수 나눗셈 (소수점 버림)
                int totalDamage = nValue + bonusDamage;
                return totalDamage.ToString();
            }
            return match.Value;
        });
        // ▼▼▼ 4. 계산 자리 표시자 처리 (Regex 사용) ▼▼▼

        // 4.1. [n*PERCENT%] 패턴 찾기 (예: [n*70%])
        // Regex 설명: \[n\*(\d+)%\]
        // \[ \] : 대괄호 문자 자체
        // n\* : "n*" 문자열
        // (\d+) : 숫자(0-9)가 1번 이상 반복되는 그룹 (이 숫자를 추출)
        // %\] : "%]" 문자열
        description = Regex.Replace(description, @"\[n\*(\d+)%\]", match =>
        {
            if (int.TryParse(match.Groups[1].Value, out int percent)) // 그룹 1 (숫자 부분) 추출
            {
                // 계산: n * percent / 100 (소수점 반올림)
                int result = Mathf.RoundToInt((float)nValue * percent / 100f);
                return result.ToString(); // 계산 결과 문자열로 반환
            }
            return match.Value; // 숫자로 변환 실패 시 원본 문자열 반환
        });

        // 4.2. [n/DIVISOR] 패턴 찾기 (예: [n/2])
        // Regex 설명: \[n\/(\d+)\]
        // \/ : 슬래시 문자 자체
        description = Regex.Replace(description, @"\[n\/(\d+)\]", match =>
        {
            if (int.TryParse(match.Groups[1].Value, out int divisor) && divisor != 0) // 0으로 나누기 방지
            {
                // 계산: n / divisor (소수점 올림)
                int result = Mathf.CeilToInt((float)nValue / divisor);
                return result.ToString();
            }
            return match.Value;
        });
        // ▲▲▲ 계산 처리 완료 ▲▲▲
        

        // 3. 합계 자리 표시자 처리 (RuneDeckManager 필요)
        if (RuneDeckManager.Instance != null)
        {
            if (description.Contains("blueSum"))
            {
                description = description.Replace("blueSum", RuneDeckManager.Instance.GetPredictedTotalDefense().ToString());
            }
            if (description.Contains("redSum"))
            {
                description = description.Replace("redSum", RuneDeckManager.Instance.GetPredictedTotalDamage().ToString());
            }
            if (description.Contains("yellowSum"))
            {
                description = description.Replace("yellowSum", RuneDeckManager.Instance.GetPredictedTotalGold().ToString());
            }
        }
        else if (description.Contains("blueSum") || description.Contains("redSum") || description.Contains("yellowSum"))
        {
            Debug.LogWarning("RuneDeckManager 인스턴스를 찾을 수 없어 합계 자리 표시자를 계산할 수 없습니다.");
            description = description.Replace("blueSum", "?").Replace("redSum", "?").Replace("yellowSum", "?");
        }


        // 2. 기본 [n] 자리 표시자 대체
        description = description.Replace("n", nValue.ToString());

        // 5. 최종 포맷된 설명으로 툴팁 텍스트 설정
        tooltipText.text = $"<b>{runeInstance.SO.displayName}</b>\n\n{description}";

        // ▼▼▼ 6. 이 코드를 추가하세요 ▼▼▼
        // 툴팁을 Hierarchy의 맨 아래로 보내 맨 위에 그리도록 함
        tooltipRect.transform.SetAsLastSibling();
        // ▲▲▲ 추가 완료 ▲▲▲

        // --- 툴팁 위치 계산 및 표시는 기존과 동일 ---
        runeTooltipPanel.SetActive(true);
        Vector3 desiredPosition = Input.mousePosition + (Vector3)tooltipOffset;
        tooltipRect.position = GetCorrectedTooltipPosition(desiredPosition);
    }


    /// <summary>
    /// 룬 설명 툴팁을 숨깁니다.
    /// </summary>
    public void HideRuneTooltip()
    {
        if (runeTooltipPanel == null) return;
        runeTooltipPanel.SetActive(false);
    }
    /// <summary>
    /// (추가된 함수) 툴팁의 위치를 받아 화면 밖으로 나가지 않도록 보정한 위치를 반환합니다.
    /// </summary>
    private Vector3 GetCorrectedTooltipPosition(Vector3 desiredPosition)
    {
        // 툴팁의 피봇(pivot)과 크기를 가져옴
        // (툴팁 UI의 Pivot을 (0, 1) - 좌측 상단으로 설정하면 계산이 더 정확합니다.)
        Vector2 tooltipPivot = tooltipRect.pivot;
        Vector2 tooltipSize = tooltipRect.sizeDelta;

        // 오른쪽 경계 체크
        if (desiredPosition.x + (tooltipSize.x * (1 - tooltipPivot.x)) > Screen.width)
        {
            desiredPosition.x = Screen.width - (tooltipSize.x * (1 - tooltipPivot.x));
        }
        // 왼쪽 경계 체크
        if (desiredPosition.x - (tooltipSize.x * tooltipPivot.x) < 0)
        {
            desiredPosition.x = (tooltipSize.x * tooltipPivot.x);
        }
        // 위쪽 경계 체크
        if (desiredPosition.y + (tooltipSize.y * (1 - tooltipPivot.y)) > Screen.height)
        {
            desiredPosition.y = Screen.height - (tooltipSize.y * (1 - tooltipPivot.y));
        }
        // 아래쪽 경계 체크
        if (desiredPosition.y - (tooltipSize.y * tooltipPivot.y) < 0)
        {
            desiredPosition.y = (tooltipSize.y * tooltipPivot.y);
        }

        return desiredPosition;
    }


    public void BindRuneDeck(Action<RuneColor> onDeckClick, Action onDrawClick)
    {
        // 각 버튼이 null이 아닌지 확인 후 리스너 설정
        if (redButton != null)
        {
            redButton.onClick.RemoveAllListeners();
            if (onDeckClick != null)
            {
                // ▼▼▼ 수정: 클릭 시 사운드 재생 후 원래 기능 실행 ▼▼▼
                redButton.onClick.AddListener(() =>
                {
                    SoundManager.Instance.PlaySfx(SfxType.RuneSelect); // 사운드 재생
                    onDeckClick(RuneColor.Red);                      // 원래 기능 호출
                });
            }
        }
        if (blueButton != null)
        {
            blueButton.onClick.RemoveAllListeners();
            if (onDeckClick != null)
            {
                // ▼▼▼ 수정 ▼▼▼
                blueButton.onClick.AddListener(() =>
                {
                    SoundManager.Instance.PlaySfx(SfxType.RuneSelect);
                    onDeckClick(RuneColor.Blue);
                });
            }
        }
        if (whiteButton != null)
        {
            whiteButton.onClick.RemoveAllListeners();
            if (onDeckClick != null)
            {
                // ▼▼▼ 수정 ▼▼▼
                whiteButton.onClick.AddListener(() =>
                {
                    SoundManager.Instance.PlaySfx(SfxType.RuneSelect);
                    onDeckClick(RuneColor.White);
                });
            }
        }
        if (yellowButton != null)
        {
            yellowButton.onClick.RemoveAllListeners();
            if (onDeckClick != null)
            {
                // ▼▼▼ 수정 ▼▼▼
                yellowButton.onClick.AddListener(() =>
                {
                    SoundManager.Instance.PlaySfx(SfxType.RuneSelect);
                    onDeckClick(RuneColor.Yellow);
                });
            }
        }

        if (drawButton != null)
        {
            drawButton.onClick.RemoveAllListeners();
            // (선택) 뽑기 버튼에도 사운드를 추가하고 싶다면 여기도 위와 같이 수정
            
            if (onDrawClick != null) drawButton.onClick.AddListener(() => {
                SoundManager.Instance.PlaySfx(SfxType.AtkBtn);
                onDrawClick();
            }); 
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

    void OnEnable()
    {
        // 씬 로드 이벤트를 구독합니다.
        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    void OnDisable()
    {
        // 씬 로드 이벤트 구독을 해제합니다.
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    /// <summary>
    /// 새로운 씬이 로드될 때마다 호출되는 함수입니다.
    /// </summary>
    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        // 로드된 씬의 이름에 "Battle"이 포함되어 있다면,
        if (scene.name.Contains("Battle"))
        {
            // 전투 UI를 활성화합니다.
            ShowRuneUI();
        }
        else
        {
            // 전투 씬이 아니라면 (예: 맵 씬), 전투 UI를 비활성화합니다.
            HideRuneUI();
        }
    }

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
    