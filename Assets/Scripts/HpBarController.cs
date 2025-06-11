using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using TMPro;

public class HpBarController : MonoBehaviour
{
    [Header("HP 관련 슬라이더")]
    [SerializeField] public Slider hpSlider;

    

    // HP바 숫자를 표시할 텍스트 컴포넌트
    [Header("HP 텍스트")]
    //[SerializeField] private TMP_Text hpText;

    [Header("방어도(Defense) 슬라이더")]
    [SerializeField] private Slider defenseSlider;

    private Vector3 offset = new Vector3(0, -250f, 0);
    private Transform targetTransform;
    private Camera mainCamera;
    private Canvas canvas;

    // 슬라이더 값 업데이트용 코루틴
    private Coroutine updateHPCoroutine;
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

        if (hpSlider != null)
        {
            hpSlider.value = hpSlider.maxValue;
        }
        else
        {
            Debug.LogError("HPBarController: hpSlider가 할당되지 않았습니다.");
        }

        if (defenseSlider != null)
        {
            defenseSlider.value = 0;
            defenseSlider.maxValue = 100; // HP와 상관없이 넉넉한 고정값으로 설정
    
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

    // 타겟 설정 (체력바가 캐릭터를 따라다닙니다.)
    public void SetTarget(Transform target)
    {
        targetTransform = target;
    }

    // 최대 체력 설정 (슬라이더와 텍스트 모두 설정)
    public void SetMaxHP(int maxHP)
    {
        if (hpSlider != null)
        {
            hpSlider.maxValue = maxHP;          
        }
    }

    // 현재 체력 설정 (애니메이션 효과와 함께 텍스트 업데이트)
    public void SetCurrentHP(int currentHP)
    {
        if (hpSlider == null || !gameObject.activeInHierarchy)
            return;

        if (updateHPCoroutine != null)
            StopCoroutine(updateHPCoroutine);

        updateHPCoroutine = StartCoroutine(UpdateHealthBar(hpSlider, currentHP));
    }

    // 방어도 최대값 설정 (필요 시)
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
        if (defenseSlider == null || !gameObject.activeInHierarchy)
            return;

        if (updateDefenseCoroutine != null)
            StopCoroutine(updateDefenseCoroutine);

        updateDefenseCoroutine = StartCoroutine(UpdateHealthBar(defenseSlider, currentDefense));
    }

    // 슬라이더 값을 부드럽게 변경하고, 동시에 텍스트도 업데이트합니다.
    private IEnumerator UpdateHealthBar(Slider slider, float targetValue)
    {
        float duration = 0.5f;
        float elapsed = 0f;
        float startValue = slider.value;

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            slider.value = Mathf.Lerp(startValue, targetValue, elapsed / duration);

            yield return null;
        }
        slider.value = targetValue;
    }

    // 오프셋 설정 (HP바의 위치 조정)
    public void SetOffset(Vector3 newOffset)
    {
        offset = newOffset;
    }
}
