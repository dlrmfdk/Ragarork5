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
    
    private Vector3 offset = new Vector3(0, -20f, 0);
    private Transform targetTransform;
    private Camera mainCamera;
    private Canvas canvas;
    private Renderer targetRenderer; // 대상의 SpriteRenderer 또는 MeshRenderer를 저장할 변수
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
        // 타겟과 렌더러가 모두 유효할 때만 위치를 계산합니다.
        if (targetTransform != null && targetRenderer != null && canvas != null)
        {
            // 1. 렌더러의 경계(bounds)를 사용하여 월드 좌표상 '발밑' 위치를 계산합니다.
            //    bounds.min.y는 렌더링된 이미지의 가장 아래쪽 Y좌표입니다.
            Vector3 feetPos_world = new Vector3(targetTransform.position.x, targetRenderer.bounds.min.y, targetTransform.position.z);

            // 2. 계산된 '발밑' 월드 좌표를 화면 좌표로 변환합니다.
            Vector3 screenPos = mainCamera.WorldToScreenPoint(feetPos_world);

            // 3. 변환된 화면 좌표에 오프셋을 더해 최종 UI 위치를 설정합니다.
            if (canvas.renderMode == RenderMode.ScreenSpaceOverlay)
            {
                transform.position = screenPos + offset;
            }
            else if (canvas.renderMode == RenderMode.ScreenSpaceCamera)
            {
                RectTransformUtility.ScreenPointToLocalPointInRectangle(
                    canvas.GetComponent<RectTransform>(), screenPos, canvas.worldCamera, out Vector2 anchoredPos);
                GetComponent<RectTransform>().anchoredPosition = anchoredPos + new Vector2(offset.x, offset.y);
            }
        }
    }

    // 타겟 설정 (체력바가 캐릭터를 따라다닙니다.)

    public void SetTarget(Transform target)
    {
        targetTransform = target;

        // 타겟이 설정될 때 Renderer 컴포넌트를 한 번만 찾아와 저장해 둡니다.
        // Spine(SkeletonAnimation)은 MeshRenderer를 사용하고, 일반 스프라이트는 SpriteRenderer를 사용합니다.
        // 두 경우 모두 부모 클래스인 Renderer 타입으로 받을 수 있습니다.
        if (target != null)
        {
            targetRenderer = target.GetComponentInChildren<Renderer>();
            if (targetRenderer == null)
            {
                Debug.LogError($"[HpBarController] {target.name} 또는 그 자식에게서 Renderer 컴포넌트(SpriteRenderer, MeshRenderer 등)를 찾을 수 없습니다.");
            }
        }
        else
        {
            targetRenderer = null;
        }
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
