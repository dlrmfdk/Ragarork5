using UnityEngine;
using UnityEngine.UI; // Slider를 사용하기 위해 필요

public class SettingsController : MonoBehaviour
{
    [Header("UI 요소")]
    [SerializeField] private Slider bgmSlider;
    [SerializeField] private Slider sfxSlider;

    // SettingWindow가 활성화될 때 호출됩니다.
    void OnEnable()
    {
        // 현재 볼륨 값을 불러와 슬라이더에 반영
        LoadSliderSettings();
    }

    // BGM 슬라이더 값이 변경될 때 호출될 함수
    public void OnBGMVolumeChanged()
    {
        if (SoundManager.Instance != null)
        {
            SoundManager.Instance.SetBGMVolume(bgmSlider.value);
        }
    }

    // SFX 슬라이더 값이 변경될 때 호출될 함수
    public void OnSFXVolumeChanged()
    {
        if (SoundManager.Instance != null)
        {
            SoundManager.Instance.SetSFXVolume(sfxSlider.value);
        }
    }

    // PlayerPrefs에 저장된 볼륨 값으로 슬라이더 UI를 초기화하는 함수
    private void LoadSliderSettings()
    {
        if (bgmSlider != null)
        {
            bgmSlider.value = PlayerPrefs.GetFloat("BGMVolume", 1.0f);
        }
        if (sfxSlider != null)
        {
            sfxSlider.value = PlayerPrefs.GetFloat("SFXVolume", 1.0f);
        }
    }
}