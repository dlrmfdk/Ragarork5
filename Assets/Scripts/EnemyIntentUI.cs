// EnemyIntentUI.cs
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class EnemyIntentUI : MonoBehaviour
{
    [SerializeField] private Image intentIconImage;
    [SerializeField] private TMP_Text valueText;

    private Transform targetEnemy;
    [Header("UI 위치 오프셋")]
    [SerializeField] private Vector3 offset = new Vector3(0, 150f, 0); // 기본값을 150으로 조절
    private Camera mainCamera;

    void Awake()
    {
        mainCamera = Camera.main;
    }

    void Update()
    {
        if (targetEnemy != null)
        {
            Vector3 screenPos = mainCamera.WorldToScreenPoint(targetEnemy.position);
            transform.position = screenPos + offset;

    
        }
    }

    public void SetTarget(Transform enemyTransform)
    {
        targetEnemy = enemyTransform;
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