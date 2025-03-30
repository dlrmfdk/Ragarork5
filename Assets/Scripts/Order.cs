using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Order : MonoBehaviour
{
    [SerializeField] Renderer[] backRenderers; //뒤쪽 렌더러 가져옴
    [SerializeField] Renderer[] middleRenderers; //중앙 ""
    [SerializeField] string sortingLayerName; //sortingLayer 이름 정해줌
    int originOrder;

    public void SetOriginOrder(int originOrder) //최초 오더 호출
    { 
        this.originOrder = originOrder; 
        SetOrder(originOrder);
    }
    public void SetMostFrontOrder(bool isMostFront) // Layer 가장 앞으로 오게 함 (카드 확대할때 등 사용)
    {
        SetOrder(isMostFront ? 100 : originOrder);
    }

    public void SetOrder(int order) //외부에서 Order 입력하면 Layer 정렬
    {
        int mulOrder = order * 10;
        foreach(var renderer in backRenderers)
        {
            renderer.sortingLayerName = sortingLayerName;
            renderer.sortingOrder = mulOrder;
        }
        foreach(var renderer in middleRenderers)
        {
            renderer.sortingLayerName = sortingLayerName;
            renderer.sortingOrder = mulOrder+1;
        }
    }

}
