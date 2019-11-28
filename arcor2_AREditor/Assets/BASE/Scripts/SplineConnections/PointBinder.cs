using System.Collections;
using System.Collections.Generic;
using BezierSolution;
using UnityEngine;

public class PointBinder : MonoBehaviour
{
    private Transform transformToBind;
    private BezierPoint point;

    // Start is called before the first frame update
    void Start()
    {
        point = transform.GetComponent<BezierPoint>();
    }

    // Update is called once per frame
    void Update()
    {
        if (transformToBind.hasChanged) {
            point.position = transform.position;
        }
    }

    public void BindPoint(Transform tf) {
        transformToBind = tf;
    }
}
