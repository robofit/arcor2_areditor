using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class OnClickCollider : MonoBehaviour
{
    public GameObject Target;
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void OnClick()
    {
        Target.GetComponent<Base.Clickable>().OnClick();
    }

    public void OnMouseDown() {
        Target.GetComponent<Base.Clickable>().OnMouseDown();
    }
}
