using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

public class ChangeScene : MonoBehaviour
{
    public AudioClip clickSound; // 버튼 클릭 사운드
    private AudioSource audioSource;

    private void Awake()
    {
        // AudioSource 컴포넌트 가져오기 또는 추가하기
        audioSource = GetComponent<AudioSource>();
        if (audioSource == null)
        {
            audioSource = gameObject.AddComponent<AudioSource>();
        }

        // AudioSource 초기 설정
        audioSource.playOnAwake = false;
    }

    public void ChangeSceneBtn()
    {
        // 버튼 소리 재생
        if (clickSound != null)
        {
            audioSource.PlayOneShot(clickSound);

            // 사운드가 끝난 후 씬 전환 실행
            StartCoroutine(WaitAndChangeScene(clickSound.length));
        }
        else
        {
            // 클릭 사운드가 없는 경우 즉시 씬 전환
            ChangeSceneImmediately();
        }
    }

    private IEnumerator WaitAndChangeScene(float waitTime)
    {
        yield return new WaitForSeconds(waitTime); // 사운드 재생이 끝날 때까지 대기
        ChangeSceneImmediately();
    }

    private void ChangeSceneImmediately()
    {
        switch (this.gameObject.name)
        {
            case "StartButton":
                SceneManager.LoadScene("MapScene");
                break;

                // 다른 버튼 케이스 추가 가능
        }
    }
}
