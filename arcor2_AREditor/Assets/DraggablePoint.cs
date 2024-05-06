/*
 * DraggablePoint
 * Author: Timotej Halenár
 * Login: xhalen00
 * Bachelor's Thesis 
 * VUT FIT 2024
 * 
 * */
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DraggablePoint : MonoBehaviour
{
    [SerializeField] private OutlineOnClick outline;
    // Start is called before the first frame update
    void Start()
    {
        
    }

    public void Highlight() {
        outline.Highlight();
    }

    public void Unhighlight() {
        outline.UnHighlight();
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
