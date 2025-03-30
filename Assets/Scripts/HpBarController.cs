using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class HpBarController : MonoBehaviour
{
    [Header("HP 관련 슬라이더")]
    [SerializeField] private Slider hpSlider;

    [Header("방어도(Defense) 슬라이더")]
    [SerializeField] private Slider defenseSlider;

     private Vector3 offset = new Vector3(0, -80f, 0);

    private Transform targetTransform;
    private Camera mainCamera;
    private Canvas canvas;

    // HP 갱신용 코루틴
    private Coroutine updateHPCoroutine;
    // 방어도 갱신용 코루틴
    private Coroutine updateDefenseCoroutine;

    void Start()
    {
        mainCamera = Camera.main;
        canvas = GetComponentInParent<Canvas>();
        if (canvas == null)
        {
            Debug.LogError("HPBarController: 부모에 Canvas가 존재하지 않습니다.");
            return;
        }

        // HP 슬라이더 초기화
        if (hpSlider != null)
        {
            hpSlider.value = hpSlider.maxValue;
        }
        else
        {
            Debug.LogError("HPBarController: hpSlider가 할당되지 않았습니다.");
        }

        // 방어도 슬라이더 초기화
        if (defenseSlider != null)
        {
            defenseSlider.value = 0;
           defenseSlider.maxValue = hpSlider != null ? hpSlider.maxValue : 100; //hp와 동일한 최대값
            // 필요하면 적정 최대값으로 설정(혹은 매번 설정)
        }
        else
        {
            Debug.LogWarning("HPBarController: defenseSlider가 할당되지 않았습니다. 방어도 표시 불가.");
        }
    }

    void Update()
    {
        if (targetTransform != null && canvas != null)
        {
            Vector3 screenPos = mainCamera.WorldToScreenPoint(targetTransform.position);

            if (canvas.renderMode == RenderMode.ScreenSpaceOverlay)
            {
                transform.position = screenPos + offset;
            }
            else if (canvas.renderMode == RenderMode.ScreenSpaceCamera)
            {
                RectTransform canvasRect = canvas.GetComponent<RectTransform>();

                Vector2 anchoredPos;
                RectTransformUtility.ScreenPointToLocalPointInRectangle(
                    canvasRect, screenPos + offset, canvas.worldCamera, out anchoredPos);

                RectTransform rectTransform = GetComponent<RectTransform>();
                rectTransform.anchoredPosition = anchoredPos;
            }
        }
    }

    // 타겟 설정
    public void SetTarget(Transform target)
    {
        targetTransform = target;
    }

    // HP 최대값 설정
    public void SetMaxHP(int maxHP)
    {
        if (hpSlider != null)
        {
            hpSlider.maxValue = maxHP;
            hpSlider.value = maxHP;
        }
    }

    // 현재 HP 설정
    public void SetCurrentHP(int currentHP)
    {
        if (hpSlider == null || !gameObject.activeInHierarchy) return;

        if (updateHPCoroutine != null)
            StopCoroutine(updateHPCoroutine);

        updateHPCoroutine = StartCoroutine(UpdateHealthBar(hpSlider, currentHP));
    }

    // 방어도(Defense) 최대값 설정 (필요 시)
    public void SetMaxDefense(int maxDefense)
    {
        if (defenseSlider != null)
        {
            defenseSlider.maxValue = maxDefense;
        }
    }

    // 현재 방어도 설정
    public void SetCurrentDefense(int currentDefense)
    {
        if (defenseSlider == null || !gameObject.activeInHierarchy) return;

        if (updateDefenseCoroutine != null)
            StopCoroutine(updateDefenseCoroutine);

        updateDefenseCoroutine = StartCoroutine(UpdateHealthBar(defenseSlider, currentDefense));
    }

    // 공통 코루틴 (슬라이더 값 부드럽게 변경)
    private IEnumerator UpdateHealthBar(Slider slider, float targetValue)
    {
        float duration = 0.5f;
        float elapsed = 0f;
        float startValue = slider.value;
        float endValue = targetValue;

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            slider.value = Mathf.Lerp(startValue, endValue, elapsed / duration);
            yield return null;
        }
        slider.value = endValue;
    }

    // 오프셋 설정
    public void SetOffset(Vector3 newOffset)
    {
        offset = newOffset;
    }
}
