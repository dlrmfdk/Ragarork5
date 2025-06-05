using UnityEngine;
using UnityEngine.UI; // Image 또는 다른 UI 컴포넌트 직접 제어 시 필요

public class TargetingUIManager : MonoBehaviour
{
    [Header("타겟 UI 설정")]
    [Tooltip("Canvas 내에 있는 타겟 표시용 UI 게임 오브젝트를 여기에 할당하세요.")]
    public GameObject targetUI; // 인스펙터에서 Canvas 내 UI 요소를 직접 할당

    void Start()
    {
        if (targetUI == null)
        {
            Debug.LogError("Target UI가 TargetingUIManager에 할당되지 않았습니다! Inspector를 확인해주세요.");
            return;
        }
        // 시작 시 타겟 UI는 숨겨둡니다.
        targetUI.SetActive(false);
    }

    /// <summary>
    /// 타겟 UI를 보여주거나 숨기고, 지정된 대상의 위치로 이동시킵니다.
    /// </summary>
    /// <param name="show">UI를 보여줄지 여부</param>
    /// <param name="targetTransform">타겟팅할 대상의 Transform. UI를 숨길 때는 null일 수 있습니다.</param>
    public void ShowTargetingUI(bool show, Transform targetTransform)
    {
        if (targetUI == null)
        {
            Debug.LogError("Target UI가 할당되지 않아 UI를 업데이트할 수 없습니다.");
            return;
        }

        targetUI.SetActive(show);

        if (show && targetTransform != null)
        {
            // 타겟 UI를 적의 머리 위 등 적절한 위치로 이동시키는 로직
            // 이 로직은 게임의 카메라 설정(Perspective/Orthographic), Canvas 설정(Screen Space - Overlay, Screen Space - Camera, World Space)에 따라 달라집니다.

            // 예시 1: Screen Space - Overlay Canvas를 사용하는 경우
            // 3D 월드 좌표를 UI 스크린 좌표로 변환하여 배치합니다.
            Vector3 screenPos = Camera.main.WorldToScreenPoint(targetTransform.position + Vector3.up * 2f); // 적 머리 위쪽으로 약간 오프셋 (값 조절 필요)

            // 타겟 UI의 RectTransform을 가져옵니다.
            RectTransform uiRect = targetUI.GetComponent<RectTransform>();
            if (uiRect != null)
            {
                // 변환된 스크린 좌표를 UI 위치로 설정합니다.
                // screenPos의 z값은 UI 요소에 직접적인 영향을 주지 않지만, 화면 앞/뒤 여부를 나타낼 수 있습니다.
                // 만약 UI가 화면 밖으로 벗어나는 것을 방지하려면 추가적인 좌표 검사 로직이 필요할 수 있습니다.
                uiRect.position = screenPos;
            }
            else
            {
                // RectTransform이 없는 일반 GameObject라면 (예: 3D 오브젝트를 타겟 UI로 사용하고 World Space Canvas가 아닌 경우)
                // 이 방식은 적절하지 않을 수 있으며, World Space Canvas를 사용하거나 다른 접근 방식이 필요합니다.
                // 현재는 UI GameObject가 RectTransform을 가질 것으로 가정합니다.
                Debug.LogWarning("Target UI에 RectTransform 컴포넌트가 없습니다. 위치를 정확히 설정하기 어려울 수 있습니다.");
                // 임시로라도 월드좌표계 UI가 아니라면 스크린 좌표에 맞춰야 합니다.
                targetUI.transform.position = screenPos; // Canvas 설정에 따라 다르게 동작할 수 있음
            }
            Debug.Log("타겟 UI 위치 업데이트: " + targetTransform.name + " at screen position " + screenPos);
        }
    }
}