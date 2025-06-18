// ParticleSystemAutoDestroy.cs (새로 만들기)
using UnityEngine;

[RequireComponent(typeof(ParticleSystem))]
public class ParticleSystemAutoDestroy : MonoBehaviour
{
    private ParticleSystem ps;

    void Start()
    {
        ps = GetComponent<ParticleSystem>();
    }

    void Update()
    {
        // 파티클 시스템이 재생 중이 아니고, 살아있는 파티클도 없다면
        if (ps.IsAlive() == false)
        {
            // 이 게임 오브젝트를 파괴합니다.
            Destroy(gameObject);
        }
    }
}