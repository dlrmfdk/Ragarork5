// PlayerInputManager.cs
using UnityEngine;
using UnityEngine.EventSystems; // UI 클릭 방지를 위해 필요
using System.Collections.Generic; // List<Enemy>를 사용하기 위해

public class PlayerInputManager : MonoBehaviour
{
    public static PlayerInputManager Instance { get; private set; }

    public bool IsTargetingMode { get; private set; } = false;
    private Enemy selectedTarget;
    private RuneSO currentRedRune; // 현재 사용 중인 빨간색 룬

    [Header("Component References")]
    [Tooltip("TargetingUIManager 인스턴스를 인스펙터에서 할당합니다.")]
    public TargetingUIManager targetingUIManager;
    // RuneDeckManager와 Player는 싱글톤 인스턴스를 사용하므로 직접 할당 필드는 제거하거나 비워둘 수 있습니다.

    [Header("Raycast Settings")]
    [Tooltip("적들이 속한 레이어를 여기에 할당하세요.")]
    public LayerMask enemyLayerMask;
    public float maxRaycastDistance = 100f; // 레이캐스트 최대 거리 (2D에서는 주로 Z축 깊이 관련)

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
        if (IsTargetingMode && Input.GetMouseButtonDown(0)) // 마우스 왼쪽 버튼 클릭
        {
            HandleTargetSelection();
        }

        // (선택적) ESC 키 등으로 타겟팅 취소 기능
        if (IsTargetingMode && Input.GetKeyDown(KeyCode.Escape))
        {
            CancelTargeting();
        }
    }

    /// <summary>
    /// 적 타겟팅 모드를 시작합니다. RuneDeckManager 등에서 호출됩니다.
    /// </summary>
    public void StartEnemyTargeting(RuneSO runeToUse)
    {
        if (targetingUIManager == null)
        {
            Debug.LogError("[PIM.StartEnemyTargeting] TargetingUIManager가 null입니다. 타겟팅을 시작할 수 없습니다.");
            return;
        }
        if (runeToUse == null || runeToUse.color != RuneColor.Red)
        {
            Debug.LogWarning("[PIM.StartEnemyTargeting] 타겟팅을 시작할 수 없습니다: 빨간색 룬이 아니거나 룬 정보가 없습니다.");
            IsTargetingMode = false; // 혹시 모를 상태를 위해 확실히 false로
            targetingUIManager.ShowTargetingUI(false, null); // 타겟 UI 숨김
            // 버튼 상태 갱신을 위해 RefreshUI 호출
            if (RuneDeckManager.Instance != null) RuneDeckManager.Instance.RefreshUI();
            return;
        }

        IsTargetingMode = true;
        currentRedRune = runeToUse;
        selectedTarget = null; // 이전 타겟 선택 초기화
        // 타겟 UI는 첫 적 클릭 시 HandleTargetSelection에서 활성화되도록 변경했으므로 여기서는 호출 X
        // targetingUIManager.ShowTargetingUI(true, null); // 이 줄은 제거됨

        Debug.Log($"[PIM.StartEnemyTargeting] 적 타겟팅 모드 시작. 현재 룬: {currentRedRune.displayName}. 적을 클릭하여 선택하세요.");

        // 타겟팅 모드 시작 시 UI 버튼 상태 변경을 위해 RuneDeckManager의 RefreshUI 호출
        if (RuneDeckManager.Instance != null)
        {
            RuneDeckManager.Instance.RefreshUI();
            Debug.Log("[PIM.StartEnemyTargeting] RuneDeckManager.RefreshUI() 호출됨 (버튼 비활성화 목적).");
        }
        else
        {
            Debug.LogWarning("[PIM.StartEnemyTargeting] RuneDeckManager.Instance가 null이라 UI 상태를 업데이트할 수 없습니다.");
        }
    }

    /// <summary>
    /// 마우스 클릭으로 적을 선택하거나 확정하는 로직을 처리합니다.
    /// </summary>
    void HandleTargetSelection()
    {
        Debug.Log("[PIM.HandleTargetSelection] 함수 호출됨.");

        // UI 요소 위를 클릭했는지 먼저 확인 (Canvas의 GraphicRaycaster 필요)
        if (EventSystem.current.IsPointerOverGameObject())
        {
            Debug.Log("[PIM.HandleTargetSelection] UI 위를 클릭했습니다. 타겟팅을 진행하지 않습니다.");
            return;
        }

        if (Camera.main == null)
        {
            Debug.LogError("[PIM.HandleTargetSelection] 메인 카메라를 찾을 수 없습니다! Camera.main이 null입니다.");
            return;
        }
        Debug.Log("[PIM.HandleTargetSelection] UI 체크 통과. 2D Raycast (GetRayIntersection) 시도 중...");

        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
        RaycastHit2D hit2D = Physics2D.GetRayIntersection(ray, maxRaycastDistance, enemyLayerMask);

        if (hit2D.collider != null) // RaycastHit2D에서는 collider로 히트 여부 판단
        {
            Debug.Log($"[PIM.HandleTargetSelection] 2D Raycast가 뭔가를 맞췄습니다: {hit2D.collider.gameObject.name} (레이어: {LayerMask.LayerToName(hit2D.collider.gameObject.layer)})");
            Enemy clickedEnemy = hit2D.collider.GetComponent<Enemy>();

            if (clickedEnemy != null)
            {
                Debug.Log($"[PIM.HandleTargetSelection] Enemy 컴포넌트를 가진 오브젝트를 클릭했습니다: {clickedEnemy.name}");
                if (selectedTarget == clickedEnemy) // 이미 선택된 적을 다시 클릭한 경우
                {
                    Debug.Log($"[PIM.HandleTargetSelection] 이미 선택된 적 '{selectedTarget.name}'을(를) 다시 클릭했습니다. 타겟 확정!");
                    EnemyTargetHit(selectedTarget);
                }
                else // 새로운 적을 클릭했거나, 아직 선택된 타겟이 없는 경우
                {
                    Debug.Log($"[PIM.HandleTargetSelection] 새로운 적 '{clickedEnemy.name}'을(를) 선택했습니다.");
                    selectedTarget = clickedEnemy;
                    if (targetingUIManager != null) targetingUIManager.ShowTargetingUI(true, selectedTarget.transform);
                }
            }
            else
            {
                Debug.Log($"[PIM.HandleTargetSelection] 2D Raycast가 맞춘 오브젝트 '{hit2D.collider.gameObject.name}'에는 Enemy 컴포넌트가 없습니다.");
                // 적이 아닌 다른 오브젝트(배경 등)를 클릭한 경우: 선택 해제 또는 무시
                // selectedTarget = null;
                // if (targetingUIManager != null) targetingUIManager.ShowTargetingUI(false, null);
            }
        }
        else
        {
            Debug.Log("[PIM.HandleTargetSelection] 2D Raycast가 아무것도 맞추지 못했습니다 (허공 클릭).");
            // 허공을 클릭한 경우: 선택 해제 또는 무시
            // selectedTarget = null;
            // if (targetingUIManager != null) targetingUIManager.ShowTargetingUI(false, null);
        }
    }

    /// <summary>
    /// 타겟이 확정되었을 때 실제 공격(룬 효과 적용)을 처리합니다.
    /// </summary>
    void EnemyTargetHit(Enemy target)
    {
        Debug.Log($"[PIM.EnemyTargetHit] 함수 호출됨. 타겟: {(target != null ? target.name : "null")}, 룬: {(currentRedRune != null ? currentRedRune.displayName : "null")}");

        if (currentRedRune != null && target != null && Player.Instance != null)
        {
            Debug.Log($"[PIM.EnemyTargetHit] {target.name}에게 {currentRedRune.displayName} 효과 적용 시도.");
            if (currentRedRune.effectSO != null)
            {
                currentRedRune.effectSO.Execute(Player.Instance, new List<Enemy> { target });
            }
            else
            {
                Debug.LogError($"[PIM.EnemyTargetHit] {currentRedRune.displayName}에 연결된 효과 SO가 없습니다!");
            }

            // RuneDeckManager에 타겟팅 공격 완료 알림
            if (RuneDeckManager.Instance != null)
            {
                RuneDeckManager.Instance.ProcessTargetedAttackComplete(currentRedRune, target);
                // RuneDeckManager.ProcessTargetedAttackComplete 내부에서 RefreshUI 및 TurnEnd가 호출될 것임
            }
            else
            {
                Debug.LogError("[PIM.EnemyTargetHit] RuneDeckManager 인스턴스가 없어 타겟팅 공격 후처리를 할 수 없습니다!");
                // 이 경우 수동으로 턴 종료 및 UI 정리 필요할 수 있음 (하지만 RuneDeckManager가 핵심이므로 이 경우는 문제)
            }
        }
        else
        {
            Debug.LogWarning("[PIM.EnemyTargetHit] 현재 룬, 타겟, 또는 플레이어가 유효하지 않아 공격을 실행할 수 없습니다.");
            // 타겟팅 실패 시에도 모드는 종료하고 UI 정리 및 턴을 넘겨야 할 수 있음
            if (RuneDeckManager.Instance != null)
            {
                RuneDeckManager.Instance.ClearSelectionsAndPrepareForNextAction(); // 선택 초기화
                if (TurnManager.Inst != null) TurnManager.Inst.EndTurn(); // 턴 강제 종료
            }
        }

        // 타겟팅 모드 종료 및 관련 상태 초기화
        IsTargetingMode = false;
        selectedTarget = null;
        currentRedRune = null;
        if (targetingUIManager != null) targetingUIManager.ShowTargetingUI(false, null); // 타겟 UI 숨김
        Debug.Log("[PIM.EnemyTargetHit] 타겟팅 상호작용 완료, 모드 종료.");

        // RuneDeckManager.ProcessTargetedAttackComplete가 RefreshUI를 호출하므로, 여기서 또 호출할 필요는 없습니다.
        // 만약 호출 순서상 필요하다면 RuneDeckManager.Instance.RefreshUI(); 추가 가능.
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
            currentRedRune = null;
            if (targetingUIManager != null) targetingUIManager.ShowTargetingUI(false, null);
            Debug.Log("[PIM.CancelTargeting] 타겟팅 취소됨.");

            // 타겟팅 취소 시 UI 버튼 상태 변경을 위해 RuneDeckManager의 RefreshUI 호출
            if (RuneDeckManager.Instance != null)
            {
                RuneDeckManager.Instance.RefreshUI();
                Debug.Log("[PIM.CancelTargeting] RuneDeckManager.RefreshUI() 호출됨.");
            }
        }
    }
}