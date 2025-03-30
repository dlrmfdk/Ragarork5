using UnityEngine;
using System.Collections.Generic;
using DG.Tweening; //do TWeen

public class Graveyard : MonoBehaviour
{
    public static Graveyard Instance { get; private set; }
    public Transform graveyardParent; // 묘지 내 카드들이 위치할 부모 트랜스폼
    public int GraveCount = 0;

    private List<GameObject> graveyardCards = new List<GameObject>();

    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
        }
    }

    public void GraveAddCard(GameObject card)
    {
        graveyardCards.Add(card);
        card.transform.SetParent(graveyardParent); //카드 묘지로 이동
        GraveCount++;
        //card.MoveTransform(new PRS(graveyardParent.position, Quaternion.identity, Vector3.zero), true, 0.5f);
        // 애니메이션 후 카드 비활성화 (옵션)
        // card.SetActive(false);
    }
}
