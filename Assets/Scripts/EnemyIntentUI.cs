// EnemyIntentUI.cs
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class EnemyIntentUI : MonoBehaviour
{
    [SerializeField] private Image intentIconImage;
    [SerializeField] private TMP_Text valueText;

    private Transform targetEnemy;


    private Renderer targetRenderer; // 대상의 SpriteRenderer 또는 MeshRenderer(Spine)를 저장할 변수

    [Header("UI 위치 오프셋")]
    [SerializeField] private Vector3 offset = new Vector3(0, 150f, 0); // 기본값을 150으로 조절
    private Camera mainCamera;

    void Awake()
    {
        mainCamera = Camera.main;
    }


    void Update()
    {
        // 타겟과 렌더러가 모두 유효할 때만 위치를 계산합니다.
        if (targetEnemy != null && targetRenderer != null && mainCamera != null)
        {
            // 1. 렌더러의 경계(bounds)를 사용하여 월드 좌표상 '머리 위' 위치를 계산합니다.
            //    bounds.max.y는 렌더링된 이미지의 가장 위쪽 Y좌표입니다.
            Vector3 headPos_world = new Vector3(targetEnemy.position.x, targetRenderer.bounds.max.y, targetEnemy.position.z);

            // 2. 계산된 '머리 위' 월드 좌표를 화면 좌표로 변환합니다.
            Vector3 screenPos = mainCamera.WorldToScreenPoint(headPos_world);

            // 3. 변환된 화면 좌표에 오프셋을 더해 최종 UI 위치를 설정합니다.
            transform.position = screenPos + offset;
        }
    }

    // EnemyIntentUI.cs

    public void SetTarget(Transform enemyTransform)
    {
        this.targetEnemy = enemyTransform;

        // 타겟이 설정될 때 Renderer 컴포넌트를 한 번만 찾아와 저장해 둡니다.
        if (targetEnemy != null)
        {
            // 자식 오브젝트에 있을 수 있는 SpriteRenderer나 MeshRenderer를 찾습니다.
            targetRenderer = targetEnemy.GetComponentInChildren<Renderer>();
            if (targetRenderer == null)
            {
                Debug.LogError($"[EnemyIntentUI] {targetEnemy.name} 또는 그 자식에게서 Renderer 컴포넌트를 찾을 수 없습니다.");
            }
        }
        else
        {
            targetRenderer = null;
        }

        Debug.Log($"[EnemyIntentUI] 타겟이 '{enemyTransform.name}'(으)로 설정되었습니다!", this.gameObject);
    }
    public void ShowIntent(EnemyActionSO action, int displayValue)
    {
        if (action == null)
        {
            Hide();
            return;
        }

        intentIconImage.sprite = action.intentIcon;

        if (action.showDamageValue)
        {
            valueText.gameObject.SetActive(true);
            string displayText = displayValue.ToString();
            if (action.hitCount > 1)
            {
                displayText += $"x{action.hitCount}";
            }
            valueText.text = displayText;
        }
        else
        {
            valueText.gameObject.SetActive(false);
        }

        gameObject.SetActive(true);
    }

    public void Hide()
    {
        gameObject.SetActive(false);
    }
}