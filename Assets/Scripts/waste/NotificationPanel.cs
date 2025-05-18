//using System.Collections;
//using System.Collections.Generic;
//using UnityEngine;
//using TMPro;
//using DG.Tweening;

//public class NotificationPanel : MonoBehaviour
//{
//    [SerializeField] TMP_Text notificationTMP;

//    public void Show(string message)
//    {
//        notificationTMP.text = message;
//        Sequence sequence = DOTween.Sequence() //커졌다가 0.9초 후에 작아지는 시퀀스
//            .Append(transform.DOScale(Vector3.one, 0.3f).SetEase(Ease.InOutQuad)) //SetEase(Ease.InOutQuad) -> 느리다가 점점 빨라지는 dotween
//            .AppendInterval(0.9f)
//            .Append(transform.DOScale(Vector3.zero, 0.3f).SetEase(Ease.InOutQuad));

//    }

//    // Start is called before the first frame update
//    void Start() => ScaleZero(); //스케일을 0으로 시작

//    [ContextMenu("ScaleOne")]
//    void ScaleOne() => transform.localScale = Vector3.one;

//    [ContextMenu("ScaleZero")]
//    public void ScaleZero() => transform.localScale = Vector3.zero;
    
        
    

//    // Update is called once per frame
//    void Update()
//    {
        
//    }
//}
