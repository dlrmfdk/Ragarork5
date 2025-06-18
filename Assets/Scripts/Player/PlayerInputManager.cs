
using UnityEngine;
using UnityEngine.EventSystems;
using System.Collections.Generic;
using System.Linq;

public class PlayerInputManager : MonoBehaviour
{
    public static PlayerInputManager Instance { get; private set; }

    public bool IsTargetingMode { get; private set; } = false;

    private Enemy selectedTarget; // 현재 선택된 적을 저장할 변수

    // CHANGED: RuneSO 리스트에서 RuneInstance 리스트로 변경
    private List<RuneInstance> currentRuneInstances;

    // (기존의 다른 변수들은 그대로 유지)
    public TargetingUIManager targetingUIManager;
    public LayerMask enemyLayerMask;
    public float maxRaycastDistance = 100f;

    void Awake()
    {
        if (Instance == null) { Instance = this; }
        else { Destroy(gameObject); }
    }

    void Update()
    {
        if (IsTargetingMode && Input.GetMouseButtonDown(0))
        {
            HandleTargetSelection();
        }

    
    }

    /// <summary>
    /// CHANGED: RuneInstance 리스트를 받아 타겟팅 모드를 시작합니다.
    /// </summary>
    public void StartEnemyTargeting(List<RuneInstance> runeInstances)
    {
        if (targetingUIManager == null || runeInstances == null || !runeInstances.Any()) return;

        IsTargetingMode = true;
        currentRuneInstances = runeInstances;
        Debug.Log($"[PIM] 적 타겟팅 모드 시작. {currentRuneInstances.Count}개의 룬 사용.");
        // 타겟팅 모드가 시작되었으니, UI를 새로고침하여 버튼들을 비활성화합니다.
        if (RuneDeckManager.Instance != null)
        {
            RuneDeckManager.Instance.RefreshUI();
        }
    }

    // PlayerInputManager.cs

    void HandleTargetSelection()
    {
        // UI 위를 클릭했다면 무시
        if (EventSystem.current.IsPointerOverGameObject())
        {
            return;
        }

        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
        RaycastHit2D hit2D = Physics2D.GetRayIntersection(ray, maxRaycastDistance, enemyLayerMask);

        if (hit2D.collider != null)
        {
            // 1. 레이캐스트가 무언가에 맞았다면, 그것이 Enemy인지 확인합니다.
            Enemy clickedEnemy = hit2D.collider.GetComponent<Enemy>();

            if (clickedEnemy != null)
            {
                // 2. 클릭한 대상이 Enemy가 맞을 경우
                if (selectedTarget == clickedEnemy)
                {
                    // 2-1. 이미 선택된 적을 다시 클릭했으므로, 공격을 확정하고 실행합니다.
                    Debug.Log($"[PIM] '{clickedEnemy.name}' 타겟 확정. 공격을 실행합니다.");
                    EnemyTargetHit(selectedTarget);
                }
                else
                {
                    // 2-2. 새로운 적을 처음 클릭했거나, 다른 적을 클릭하여 타겟을 변경합니다.
                    Debug.Log($"[PIM] '{clickedEnemy.name}'을(를) 새로운 타겟으로 선택합니다.");
                    selectedTarget = clickedEnemy;

                    // 타겟팅 UI를 해당 적에게 표시합니다.
                    if (targetingUIManager != null)
                    {
                        targetingUIManager.ShowTargetingUI(true, selectedTarget.transform);
                    }
                }
            }

        }
    }

    /// <summary>
    /// CHANGED: 확정된 타겟에게 RuneInstance의 효과를 적용합니다.
    /// </summary>
    private void EnemyTargetHit(Enemy target)
    {

        if (Player.Instance != null)
        {
            //플레이어 공격 시 이펙트
            //Vector3 playerEffectPos = Player.Instance.transform.position + Player.Instance.effectOffset;
            //EffectManager.Instance.PlayEffect(EffectType.PlayerAttackCast, playerEffectPos);
            Player.Instance.AtkAni();
        }
        // RuneDeckManager에게 공격이 완료되었음을 알리고, 후처리를 위임합니다.
        // ProcessTargetedAttackComplete 내부에서 실제 공격이 이루어집니다.
        if (RuneDeckManager.Instance != null)
        {
            // 대표 룬(보통 첫 번째 룬)의 SO를 전달하여 어떤 종류의 공격인지 알립니다.
            RuneSO representativeRuneSO = currentRuneInstances.First().SO;
            RuneDeckManager.Instance.ProcessTargetedAttackComplete(representativeRuneSO, target);
        }

        EndTargetingMode();
    }


    private void EndTargetingMode()
    {
        IsTargetingMode = false;
        currentRuneInstances = null;
        if (targetingUIManager != null) targetingUIManager.ShowTargetingUI(false, null);

        // 타겟팅 모드가 종료되었으니, UI를 새로고침하여 버튼들을 다시 활성화합니다.
        if (RuneDeckManager.Instance != null)
        {
            RuneDeckManager.Instance.RefreshUI();
        }
    }

}