using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GUIHelpers2D : Base.Singleton<GUIHelpers2D>
{
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void ShowNotification(string message)
    {
        SSTools.ShowMessage(message, SSTools.Position.bottom, SSTools.Time.threeSecond);
    }
}
