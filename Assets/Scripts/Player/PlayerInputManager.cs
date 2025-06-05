// PlayerInputManager.cs
using UnityEngine;
using UnityEngine.EventSystems;
using System.Collections.Generic;

public class PlayerInputManager : MonoBehaviour
{
    public static PlayerInputManager Instance { get; private set; }

    public bool IsTargetingMode { get; private set; } = false;
    private Enemy selectedTarget;
    private RuneSO currentRedRune;

    public TargetingUIManager targetingUIManager;
    public RuneDeckManager runeDeckManager; // 추가되었던 참조
    public Player player;

    [Header("Raycast Settings")] // 레이캐스트 관련 설정을 인스펙터에서 편하게 관리
    public LayerMask enemyLayerMask; // 적들이 속한 레이어를 여기에 할당
    public float maxRaycastDistance = 100f; // 레이캐스트 최대 거리

    void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
    }

    void Update()
    {
        if (IsTargetingMode && Input.GetMouseButtonDown(0))
        {
            // Debug.Log("[PIM.Update] Mouse button down in targeting mode."); // Update에서 클릭 감지 확인용
            HandleTargetSelection();
        }
    }

    public void StartEnemyTargeting(RuneSO runeToUse)
    {
        if (runeToUse == null || runeToUse.color != RuneColor.Red)
        {
            Debug.LogWarning("[PIM.StartEnemyTargeting] 타겟팅을 시작할 수 없습니다: 빨간색 룬이 아니거나 룬 정보가 없습니다.");
            IsTargetingMode = false;
            if (targetingUIManager != null) targetingUIManager.ShowTargetingUI(false, null);
            return;
        }

        IsTargetingMode = true;
        currentRedRune = runeToUse;
        selectedTarget = null;
        if (targetingUIManager != null) targetingUIManager.ShowTargetingUI(true, null);
        Debug.Log($"[PIM.StartEnemyTargeting] 적 타겟팅 모드 시작. 현재 룬: {currentRedRune.displayName}");
    }

    void HandleTargetSelection()
    {
        Debug.Log("[PIM.HandleTargetSelection] 함수 호출됨.");

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
        // 2D Raycast로 변경: Physics2D.GetRayIntersection 또는 Physics2D.Raycast 사용
        // 여기서는 Physics2D.GetRayIntersection을 사용하는 예시를 보여드립니다.
        // 이 함수는 카메라에서 쏜 Ray와 교차하는 첫 번째 Collider2D를 반환합니다.
        RaycastHit2D hit2D = Physics2D.GetRayIntersection(ray, maxRaycastDistance, enemyLayerMask);

        if (hit2D.collider != null) // RaycastHit2D에서는 collider로 히트 여부 판단
        {
            Debug.Log($"[PIM.HandleTargetSelection] 2D Raycast가 뭔가를 맞췄습니다: {hit2D.collider.gameObject.name} (레이어: {LayerMask.LayerToName(hit2D.collider.gameObject.layer)})");

            Enemy clickedEnemy = hit2D.collider.GetComponent<Enemy>();

            if (clickedEnemy != null)
            {
                Debug.Log($"[PIM.HandleTargetSelection] Enemy 컴포넌트를 가진 오브젝트를 클릭했습니다: {clickedEnemy.name}");
                if (selectedTarget == clickedEnemy)
                {
                    Debug.Log($"[PIM.HandleTargetSelection] 이미 선택된 적 '{selectedTarget.name}'을(를) 다시 클릭했습니다. 타겟 확정!");
                    EnemyTargetHit(selectedTarget);
                }
                else
                {
                    Debug.Log($"[PIM.HandleTargetSelection] 새로운 적 '{clickedEnemy.name}'을(를) 선택했습니다.");
                    selectedTarget = clickedEnemy;
                    if (targetingUIManager != null) targetingUIManager.ShowTargetingUI(true, selectedTarget.transform);
                }
            }
            else
            {
                Debug.Log($"[PIM.HandleTargetSelection] 2D Raycast가 맞춘 오브젝트 '{hit2D.collider.gameObject.name}'에는 Enemy 컴포넌트가 없습니다.");
            }
        }
        else
        {
            Debug.Log("[PIM.HandleTargetSelection] 2D Raycast가 아무것도 맞추지 못했습니다.");
        }
    }

    void EnemyTargetHit(Enemy target)
    {
        // ... (이전 EnemyTargetHit 함수 내용과 거의 동일하게 유지, RuneDeckManager 호출 부분 포함) ...
        // 로그 추가하여 이 함수가 호출되는지 명확히 확인
        Debug.Log($"[PIM.EnemyTargetHit] 함수 호출됨. 타겟: {target.name}, 룬: {currentRedRune.displayName}");

        if (currentRedRune != null && target != null && player != null)
        {
            Debug.Log($"[PIM.EnemyTargetHit] {target.name}에게 {currentRedRune.displayName} 효과 적용 시도.");
            if (currentRedRune.effectSO != null)
            {
                currentRedRune.effectSO.Execute(player, new List<Enemy> { target });
            }
            else
            {
                Debug.LogError($"[PIM.EnemyTargetHit] {currentRedRune.displayName}에 연결된 효과 SO가 없습니다!");
            }

            if (RuneDeckManager.Instance != null)
            {
                RuneDeckManager.Instance.ProcessTargetedAttackComplete(currentRedRune, target);
            }
            else
            {
                Debug.LogError("[PIM.EnemyTargetHit] RuneDeckManager 인스턴스가 없어 타겟팅 공격 후처리를 할 수 없습니다!");
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
        currentRedRune = null;
        if (targetingUIManager != null) targetingUIManager.ShowTargetingUI(false, null);
        Debug.Log("[PIM.EnemyTargetHit] 타겟팅 상호작용 완료, 모드 종료.");
    }


    public void CancelTargeting()
    {
        if (IsTargetingMode)
        {
            IsTargetingMode = false;
            selectedTarget = null;
            currentRedRune = null;
            if (targetingUIManager != null) targetingUIManager.ShowTargetingUI(false, null);
            Debug.Log("[PIM.CancelTargeting] 타겟팅 취소됨.");
        }
    }
}