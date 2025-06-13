// RuneSlotUI.cs (Áß¾Ó ÇÚµå ½½·Ô Àü¿ë)
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class RuneSlotUI : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    [SerializeField] private Image iconImage;
    private RuneInstance currentInstance;

    public void Setup(RuneInstance instance)
    {
        this.currentInstance = instance;
        if (instance != null && instance.SO != null)
        {
            iconImage.sprite = instance.SO.icon;
            iconImage.enabled = true;
        }
        else
        {
            iconImage.enabled = false;
        }
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        if (currentInstance != null && UIManager.Instance != null)
        {
            UIManager.Instance.ShowRuneTooltip(currentInstance);
        }
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        if (UIManager.Instance != null)
        {
            UIManager.Instance.HideRuneTooltip();
        }
    }
}