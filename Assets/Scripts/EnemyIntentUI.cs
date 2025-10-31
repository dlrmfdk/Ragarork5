// EnemyIntentUI.cs (툴팁 기능 추가 버전)
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.EventSystems; // ▼▼▼ 1. 마우스 이벤트 감지를 위해 이 using문을 추가하세요 ▼▼▼

// ▼▼▼ 2. IPointerEnterHandler, IPointerExitHandler 인터페이스 2개를 추가하세요 ▼▼▼
public class EnemyIntentUI : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    [SerializeField] private Image intentIconImage;
    [SerializeField] private TMP_Text valueText;

    private Transform targetEnemy;
    private Renderer targetRenderer;
    [SerializeField] private Vector3 offset = new Vector3(0, 150f, 0);
    private Camera mainCamera;

    // ▼▼▼ 3. 툴팁에 표시할 내용을 저장할 변수 2개를 추가하세요 ▼▼▼
    private string currentTitle = "";
    private string currentDescription = "";
    // ▲▲▲ 추가 완료 ▲▲▲

    void Awake()
    {
        mainCamera = Camera.main;
    }

    void Update()
    {
        // ... (Update 함수 내용은 기존과 동일합니다) ...
        if (targetEnemy != null && targetRenderer != null && mainCamera != null)
        {
            Vector3 headPos_world = new Vector3(targetEnemy.position.x, targetRenderer.bounds.max.y, targetEnemy.position.z);
            Vector3 screenPos = mainCamera.WorldToScreenPoint(headPos_world);
            transform.position = screenPos + offset;
        }
    }

    public void SetTarget(Transform enemyTransform)
    {
        // ... (SetTarget 함수 내용은 기존과 동일합니다) ...
        this.targetEnemy = enemyTransform;
        if (targetEnemy != null)
        {
            targetRenderer = targetEnemy.GetComponentInChildren<Renderer>();
            if (targetRenderer == null)
            {
                Debug.LogError($"[EnemyIntentUI] {targetEnemy.name} 또는 그 자식에게서 Renderer 컴포넌트를 찾을 수 없습니다.");
            }
        }
        else { targetRenderer = null; }
        Debug.Log($"[EnemyIntentUI] 타겟이 '{enemyTransform.name}'(으)로 설정되었습니다!", this.gameObject);
    }

    // ▼▼▼ 4. ShowIntent 함수의 매개변수(괄호 안)를 수정합니다 ▼▼▼
    public void ShowIntent(EnemyActionSO action, int displayValue, string title, string description)
    {
        if (action == null)
        {
            Hide();
            return;
        }

        // (기존 아이콘/텍스트 표시 로직)
        intentIconImage.sprite = action.intentIcon;
        if (action.showDamageValue)
        {
            valueText.gameObject.SetActive(true);
            string displayText = displayValue.ToString();
            if (action.hitCount > 1) { displayText += $"x{action.hitCount}"; }
            valueText.text = displayText;
        }
        else { valueText.gameObject.SetActive(false); }
        gameObject.SetActive(true);

        // [추가] 툴팁 내용 저장
        this.currentTitle = title;
        this.currentDescription = description;
    }
    // ▲▲▲ 수정 완료 ▲▲▲

    public void Hide()
    {
        gameObject.SetActive(false);
        // ▼▼▼ 5. 숨길 때 툴팁 정보도 초기화합니다 ▼▼▼
        this.currentTitle = "";
        this.currentDescription = "";
        // ▲▲▲ 추가 완료 ▲▲▲
    }

    // ▼▼▼ 6. 마우스 이벤트 감지 함수 2개를 '새로 추가'합니다 ▼▼▼
    /// <summary>
    /// 마우스가 이 UI 요소(아이콘) 위로 올라왔을 때 호출됩니다.
    /// </summary>
    public void OnPointerEnter(PointerEventData eventData)
    {
        // 저장된 툴팁 내용이 있고, UIManager가 존재하면
        if (UIManager.Instance != null && !string.IsNullOrEmpty(currentDescription))
        {
            // UIManager에게 툴팁을 띄워달라고 요청
            UIManager.Instance.ShowSimpleTooltip(currentTitle, currentDescription);
        }
    }

    /// <summary>
    /// 마우스가 이 UI 요소(아이콘) 밖으로 나갔을 때 호출됩니다.
    /// </summary>
    public void OnPointerExit(PointerEventData eventData)
    {
        // UIManager가 존재하면 툴팁을 숨깁니다.
        if (UIManager.Instance != null)
        {
            UIManager.Instance.HideRuneTooltip();
        }
    }
    // ▲▲▲ 함수 추가 완료 ▲▲▲
}