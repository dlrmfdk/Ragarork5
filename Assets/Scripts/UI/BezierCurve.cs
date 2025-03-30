//using System.Collections;
//using System.Collections.Generic;
//using UnityEngine;
//using UnityEngine.UI;

//public class BezierCurve : MonoBehaviour
//{
//    public Transform p0, p1, p2;

//    [SerializeField]
//    private Image[] bezierCurveImage;

//    [SerializeField]
//    private float minSize = 0.1f, maxSize = 0.5f;
//    [SerializeField]
//    private float arrowSize = 0.7f;

//    [SerializeField]
//    private Color highlightColor = Color.white, originColor = Color.white;

//    private void Awake()
//    {
//        for (int i = 0; i < bezierCurveImage.Length - 1; i++)
//        {
//            bezierCurveImage[i].transform.localScale = Vector3.one * Mathf.Lerp(minSize, maxSize, (float)i / (bezierCurveImage.Length - 1));
//        }
//        bezierCurveImage[bezierCurveImage.Length - 1].transform.localScale = Vector3.one * arrowSize;
//    }

//    public Vector3 Bezier(float t)
//    {
//        float oneMinusT = 1 - t;
//        float oneMinusTPower = Mathf.Pow(oneMinusT, 2);
//        float tPower = Mathf.Pow(t, 2);

//        Vector3 result = oneMinusTPower * p0.position + 2 * t * oneMinusT * p1.position + tPower * p2.position;

//        return result;
//    }

//    public void Highlight(bool flag)
//    {
//        for (int i = 0; i < bezierCurveImage.Length; i++)
//        {
//            if (flag)
//                bezierCurveImage[i].color = highlightColor;
//            else
//                bezierCurveImage[i].color = originColor;
//        }
//    }

//    private void Update()
//    {
//        for (int i = 0; i < bezierCurveImage.Length; i++)
//        {
//            Vector3 pos = Bezier((float)i / (bezierCurveImage.Length - 1));
//            bezierCurveImage[i].transform.position = pos;

//            if (i != 0)
//            {
//                Vector3 dir = (bezierCurveImage[i].transform.position - bezierCurveImage[i - 1].transform.position).normalized;
//                float theta = Mathf.Atan2(dir.y, dir.x) * Mathf.Rad2Deg;
//                bezierCurveImage[i].transform.localEulerAngles = new Vector3(0f, 0f, theta);
//            }
//        }
//    }

//    public void UpdatePoints(Vector3 startPos, Vector3 controlPos, Vector3 endPos)
//    {
//        p0.position = startPos;
//        p1.position = controlPos;
//        p2.position = endPos;
//    }
//}
