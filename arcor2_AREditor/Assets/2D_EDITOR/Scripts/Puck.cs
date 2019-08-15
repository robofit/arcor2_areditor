using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Puck : MonoBehaviour
{

    [System.NonSerialized]
    public string type;
    public string id;
    MenuManager MenuManager;
    GameManager GameManager;
    public Action Action;
    // Start is called before the first frame update
    void Start()
    {
        MenuManager = GameObject.Find("_MenuManager").GetComponent<MenuManager>();
        GameManager = GameObject.Find("_GameManager").GetComponent<GameManager>();
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void Init(string action_id, Action action, bool updateProject = true)
    {
        if (GameManager == null)
            GameManager = GameObject.Find("_GameManager").GetComponent<GameManager>();
        const string glyphs = "0123456789";
        string newId = action_id;
        for (int i = 0; i < 4; i++)
        {
            newId += glyphs[Random.Range(0, glyphs.Length)];
        }
        Action = action;
        
        UpdateId(newId, updateProject);
        type = action_id;
    }

    public void UpdateId(string newId, bool updateProject=true)
    {
        id = newId;
        Action.Name = newId;
        gameObject.GetComponentInChildren<Text>().text = id;
        Debug.Log(GameManager);
        if (updateProject)
            GameManager.UpdateProject();
    }

    void Touch()
    {
        if (Action == null)
        {
            return;
        }
        MenuManager.PuckMenu.GetComponent<PuckMenu>().UpdateMenu(Action, gameObject);
        MenuManager.ShowMenu(MenuManager.PuckMenu);
    }

    public void DeletePuck(bool updateProject=true)
    {
        foreach (InputOutput io in GetComponentsInChildren<InputOutput>())
        {
            if (io.Connection != null)
                Destroy(io.Connection.gameObject);
        }
        gameObject.SetActive(false);
        Destroy(gameObject);
        if (updateProject)
            GameManager.UpdateProject();
    }

    
}
