using UnityEngine;

public class AnimationTrigger : MonoBehaviour
{
    private Animator animator;

    void Start()
    {
        // Animator 컴포넌트를 가져옵니다.
        animator = GetComponent<Animator>();
    }

    // 이 함수가 호출되면 애니메이션 재생
    public void PlayMyAnimation()
    {
        animator.SetTrigger("PlayAnimation");
    }
}
