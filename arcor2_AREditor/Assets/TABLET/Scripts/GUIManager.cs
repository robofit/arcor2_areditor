using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GUIManager : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void ConnectToServer() {
#if UNITY_EDITOR
        Base.GameManager.Instance.ConnectToSever("pckapinus", 6789);
#else
        Base.GameManager.Instance.ConnectToSever("pckapinus", 6789);
#endif
    }
}
