// 예시 (캔버스용 Text 사용 시)
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class HpbarText : MonoBehaviour
{
    public TMP_Text hpText;

    private HpBarController hpBarController;

    void Start()
    {
        hpBarController = GetComponent<HpBarController>();

        if (hpText == null)
            Debug.LogError("hpText가 할당되지 않았습니다.");
        if (hpBarController == null)
            Debug.LogError("HpBarController 컴포넌트를 찾을 수 없습니다.");
    }

    void Update()
    {
        if (hpText != null && hpBarController != null && hpBarController.hpSlider != null)
        {
            hpText.text = (int)hpBarController.hpSlider.value + " / " + (int)hpBarController.hpSlider.maxValue;
        }
    }
}
