using System.Collections.Generic;
using DanielLochner.Assets.SimpleSideMenu;
using UnityEngine;
using UnityEngine.UI;

public class MenuManager : Base.Singleton<MenuManager> {
    public GameObject InteractiveObjectMenu, ActionPointMenuTester, ActionPointMenu, PuckMenu, MainMenu, NewObjectTypeMenu;
    GameObject MenuOpened;
    ActionsManager ActionsManager;
    public GameObject ActionPointMenuPrefab, ButtonPrefab;

    public Dictionary<string, GameObject> ActionObjectsMenus = new Dictionary<string, GameObject>();
    // Start is called before the first frame update
    void Start() {
        ActionsManager = GameObject.Find("_ActionsManager").GetComponent<ActionsManager>();
    }

    // Update is called once per frame
    void Update() {

    }

    public void UpdateActionObjectMenu(Base.ActionObjectMetadata ao) {
        /*if (!ActionObjectsMenus.TryGetValue(ao.Type, out GameObject menu))
        {
            menu = Instantiate(ActionPointMenuPrefab);
            menu.transform.SetParent(transform.parent);
            menu.transform.localScale = new Vector3(1, 1, 1);
            menu.GetComponent<RectTransform>().anchorMin = new Vector2(1, 0);
            menu.GetComponent<RectTransform>().anchorMax = new Vector2(1, 1);
            menu.GetComponent<RectTransform>().pivot = new Vector2(0, 0.5f);
            ActionObjectsMenus.Add(ao.Type, menu);
        }
        foreach (Button b in menu.transform.Find("Layout").gameObject.GetComponentsInChildren<Button>())
        {
            Destroy(b.gameObject);
        }
        foreach (ActionMetadata action in ActionsManager.GetAllActionsOfObject(ao.Type))
        {
            GameObject btnGO = Instantiate(ButtonPrefab);
            btnGO.transform.SetParent(menu.transform.Find("Layout"));
            btnGO.transform.localScale = new Vector3(1, 1, 1);
            Button btn = btnGO.GetComponent<Button>();
            btn.GetComponentInChildren<Text>().text = action.Name;
            btn.onClick.AddListener(() => menu.GetComponent<ActionPointMenu>().CreatePuck(action.Name));
        }*/
    }

    public GameObject GetAPMenuByType(string type) {
        Debug.Log(type);
        if (ActionObjectsMenus.TryGetValue(type, out GameObject menu))
            return menu;
        return null;
    }

    public bool IsAnyMenuOpened() {
        return MenuOpened != null;
    }

    public void ShowMenu(GameObject Menu, string Headline = "") {
        //Debug.Log(Menu); 
        if (Menu == null)
            return;
        HideAllMenus();
        if (Headline != "")
            Menu.transform.Find("Layout").Find("TopText").GetComponent<InputField>().text = Headline;
        Menu.GetComponent<SimpleSideMenu>().Open();
        MenuOpened = Menu;
    }

    public void HideAllMenus() {
        if (InteractiveObjectMenu.GetComponent<SimpleSideMenu>().CurrentState == SimpleSideMenu.State.Open) {
            InteractiveObjectMenu.GetComponent<SimpleSideMenu>().Close();
        }
        if (ActionPointMenu.GetComponent<SimpleSideMenu>().CurrentState == SimpleSideMenu.State.Open) {
            ActionPointMenu.GetComponent<SimpleSideMenu>().Close();
        }
        if (PuckMenu.GetComponent<SimpleSideMenu>().CurrentState == SimpleSideMenu.State.Open) {
            PuckMenu.GetComponent<SimpleSideMenu>().Close();
        }
        /*foreach (GameObject menu in ActionObjectsMenus.Values)
        {
            if (menu.GetComponent<SimpleSideMenu>().CurrentState == SimpleSideMenu.State.Open)
            {
                menu.GetComponent<SimpleSideMenu>().Close();
            }
        }*/
    }

    public void HideMenu() {
        if (MenuOpened != null) {
            MenuOpened.SetActive(false);
        }
    }
}
