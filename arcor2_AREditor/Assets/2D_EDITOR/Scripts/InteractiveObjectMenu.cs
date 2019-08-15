using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class InteractiveObjectMenu : MonoBehaviour
{
    public GameObject APPrefab, CurrentObject;
    GameManager GameManager;
   
    // Start is called before the first frame update
    void Start()
    {
        GameManager = GameObject.Find("_GameManager").GetComponent<GameManager>();
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void CreateNewAP()
    {
        if (CurrentObject == null)
        {
            return;
        }
        GameManager.SpawnActionPoint(CurrentObject.GetComponent<InteractiveObject>());
        
    }

    public void SaveID(string new_id)
    {
        CurrentObject.GetComponent<InteractiveObject>().Id = new_id;
        GameManager.UpdateScene();
    }

    public void DeleteIO()
    {
        CurrentObject.GetComponent<InteractiveObject>().DeleteIO();
        CurrentObject = null;
    }
}
