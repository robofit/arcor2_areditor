using System.Collections;
using System.Collections.Generic;
using BezierSolution;
using UnityEngine;

public class SplineConnection : MonoBehaviour
{
    public GameObject FirstObject;
    public GameObject SecondObject;

    private BezierSpline spline;
    private BezierPoint point1;
    private BezierPoint point2;

    // Start is called before the first frame update
    void Start()
    {
        spline = GetComponent<BezierSpline>();
        point1 = transform.GetChild(0).GetComponent<BezierPoint>();
        point2 = transform.GetChild(1).GetComponent<BezierPoint>();
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void BindConnection(GameObject firstObject, GameObject secondObject) {
        FirstObject = firstObject;
        SecondObject = secondObject;

        point1.gameObject.AddComponent<PointBinder>().BindPoint(FirstObject.transform);
        point2.gameObject.AddComponent<PointBinder>().BindPoint(SecondObject.transform);
    }
}
