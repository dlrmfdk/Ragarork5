// PlayerInputManager.cs
using UnityEngine;
using UnityEngine.EventSystems; // UI 클릭 방지를 위해 필요
using System.Collections.Generic; // List<Enemy>를 사용하기 위해
using System.Linq; // ToList() 사용을 위해 추가

public class PlayerInputManager : MonoBehaviour
{
    public static PlayerInputManager Instance { get; private set; }

    public bool IsTargetingMode { get; private set; } = false;
    private Enemy selectedTarget;

    // 단일 룬에서 룬 리스트로 변경됨
    private List<RuneSO> currentRedRunes;

    [Header("Component References")]
    [Tooltip("TargetingUIManager 인스턴스를 인스펙터에서 할당합니다.")]
    public TargetingUIManager targetingUIManager;
    // Player는 싱글톤 인스턴스를 사용하므로 직접 할당 필드는 제거하거나 비워둘 수 있습니다.
    // public Player player; 

    [Header("Raycast Settings")]
    [Tooltip("적들이 속한 레이어를 여기에 할당하세요.")]
    public LayerMask enemyLayerMask;
    public float maxRaycastDistance = 100f; // 레이캐스트 최대 거리

    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
            return;
        }

        if (targetingUIManager == null)
        {
            Debug.LogError("[PIM.Awake] TargetingUIManager가 할당되지 않았습니다!");
        }
    }

    void Update()
    {
        if (IsTargetingMode && Input.GetMouseButtonDown(0))
        {
            HandleTargetSelection();
        }

        if (IsTargetingMode && Input.GetKeyDown(KeyCode.Escape))
        {
            CancelTargeting();
        }
    }

    /// <summary>
    /// 여러 개의 빨간색 룬으로 적 타겟팅 모드를 시작합니다.
    /// </summary>
    public void StartEnemyTargeting(List<RuneSO> runesToUse)
    {
        if (targetingUIManager == null)
        {
            Debug.LogError("[PIM.StartEnemyTargeting] TargetingUIManager가 null입니다. 타겟팅을 시작할 수 없습니다.");
            return;
        }
        if (runesToUse == null || runesToUse.Count == 0)
        {
            Debug.LogWarning("[PIM.StartEnemyTargeting] 타겟팅할 룬이 없습니다.");
            return;
        }

        IsTargetingMode = true;
        currentRedRunes = runesToUse; // 룬 리스트 저장
        selectedTarget = null;

        Debug.Log($"[PIM.StartEnemyTargeting] 적 타겟팅 모드 시작. {currentRedRunes.Count}개의 빨간 룬 사용.");

        if (RuneDeckManager.Instance != null)
        {
            RuneDeckManager.Instance.RefreshUI();
        }
    }

    /// <summary>
    /// 마우스 클릭으로 적을 선택하거나 확정하는 로직을 처리합니다.
    /// </summary>
    void HandleTargetSelection()
    {
        if (EventSystem.current.IsPointerOverGameObject())
        {
            return;
        }

        if (Camera.main == null)
        {
            Debug.LogError("[PIM.HandleTargetSelection] 메인 카메라를 찾을 수 없습니다! Camera.main이 null입니다.");
            return;
        }

        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
        RaycastHit2D hit2D = Physics2D.GetRayIntersection(ray, maxRaycastDistance, enemyLayerMask);

        if (hit2D.collider != null)
        {
            Enemy clickedEnemy = hit2D.collider.GetComponent<Enemy>();

            if (clickedEnemy != null)
            {
                if (selectedTarget == clickedEnemy)
                {
                    EnemyTargetHit(selectedTarget);
                }
                else
                {
                    selectedTarget = clickedEnemy;
                    if (targetingUIManager != null) targetingUIManager.ShowTargetingUI(true, selectedTarget.transform);
                }
            }
        }
    }

    /// <summary>
    /// 타겟이 확정되었을 때, 가지고 있던 모든 빨간색 룬의 효과를 순차적으로 적용합니다.
    /// </summary>
    void EnemyTargetHit(Enemy target)
    {
        if (currentRedRunes != null && currentRedRunes.Count > 0 && target != null && Player.Instance != null)
        {
            Debug.Log($"{target.name}에게 {currentRedRunes.Count}개의 빨간 룬 효과를 순차적으로 적용합니다.");

            foreach (var runeSO in currentRedRunes)
            {
                if (runeSO != null && runeSO.effectSO != null)
                {
                    if (target.currentHealth <= 0)
                    {
                        Debug.Log("대상이 사망하여 나머지 룬 효과 적용을 중단합니다.");
                        break;
                    }
                    Debug.Log($" - '{runeSO.displayName}' 효과 적용.");
                    runeSO.effectSO.Execute(Player.Instance, new List<Enemy> { target });
                }
            }

            if (RuneDeckManager.Instance != null)
            {
                RuneDeckManager.Instance.ProcessTargetedAttackComplete(currentRedRunes.First(), target);
            }
        }
        else
        {
            Debug.LogWarning("[PIM.EnemyTargetHit] 현재 룬, 타겟, 또는 플레이어가 유효하지 않아 공격을 실행할 수 없습니다.");
            if (RuneDeckManager.Instance != null)
            {
                RuneDeckManager.Instance.ClearSelectionsAndPrepareForNextAction();
                if (TurnManager.Inst != null) TurnManager.Inst.EndTurn();
            }
        }

        IsTargetingMode = false;
        selectedTarget = null;
        currentRedRunes = null;
        if (targetingUIManager != null) targetingUIManager.ShowTargetingUI(false, null);
        Debug.Log("[PIM.EnemyTargetHit] 타겟팅 상호작용 완료, 모드 종료.");
    }

    /// <summary>
    /// 타겟팅 모드를 취소합니다. (예: ESC 키 또는 취소 버튼)
    /// </summary>
    public void CancelTargeting()
    {
        if (IsTargetingMode)
        {
            IsTargetingMode = false;
            selectedTarget = null;

        
            currentRedRunes = null;
         

            if (targetingUIManager != null) targetingUIManager.ShowTargetingUI(false, null);
            Debug.Log("[PIM.CancelTargeting] 타겟팅 취소됨.");

            if (RuneDeckManager.Instance != null)
            {
                RuneDeckManager.Instance.RefreshUI();
                Debug.Log("[PIM.CancelTargeting] RuneDeckManager.RefreshUI() 호출됨.");
            }
        }
    }
}
