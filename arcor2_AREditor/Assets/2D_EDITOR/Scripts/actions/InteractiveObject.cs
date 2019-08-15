using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;


public class InteractiveObject : MonoBehaviour
{
    public string type;
    [System.NonSerialized]
    public Vector3 position;
    [System.NonSerialized]
    public Quaternion orientation = new Quaternion(0,0,0,1);
    [System.NonSerialized]
    public GameObject InteractiveObjectMenu;
    [System.NonSerialized]
    public GameObject ConnectionPrefab;

    public GameObject _ActionPoints;
    MenuManager _MenuManager;
    [System.NonSerialized]
    public int CounterAP = 0;
    private Vector3 offset;
    GameManager GameManager;
    string id;


    public ActionObjectMetadata ActionObjectMetadata;

    public string Id { get => id; set
        {
            id = value;
        }
    }

    private void Start()
    {
        GameManager = GameObject.Find("_GameManager").GetComponent<GameManager>();
        _ActionPoints = transform.Find("ActionPoints").gameObject;
        _MenuManager = GameObject.Find("_MenuManager").gameObject.GetComponent<MenuManager>();
        InteractiveObjectMenu = _MenuManager.InteractiveObjectMenu;
        ConnectionPrefab = GameManager.ConnectionPrefab;
    }

    void Update()
    {

    }

    void Touch()
    {
        _MenuManager.ShowMenu(InteractiveObjectMenu, Id);
        InteractiveObjectMenu.GetComponent<InteractiveObjectMenu>().CurrentObject = gameObject;
    }

    

    void OnMouseDown()
    {
       
        offset = gameObject.transform.position -
            Camera.main.ScreenToWorldPoint(new Vector3(Input.mousePosition.x, Input.mousePosition.y, 10.0f));
    }

    void OnMouseDrag()
    {
        Vector3 newPosition = new Vector3(Input.mousePosition.x, Input.mousePosition.y, 10.0f);
        transform.position = Camera.main.ScreenToWorldPoint(newPosition) + offset;
    }

    void OnMouseUp()
    {
        GameManager.UpdateScene();
    }

    public void DeleteIO(bool updateScene=true)
    {
        foreach (ActionPoint ap in GetComponentsInChildren<ActionPoint>())
        {
            ap.DeleteAP(false);
        }
        gameObject.SetActive(false);
        Destroy(gameObject);
        if (updateScene)
            GameManager.UpdateScene();
    }

}