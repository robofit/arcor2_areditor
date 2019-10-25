using System.Collections.Generic;
using DanielLochner.Assets.SimpleSideMenu;
using UnityEngine;
using UnityEngine.UI;

public class MenuManager : Base.Singleton<MenuManager> {
    public GameObject InteractiveObjectMenu, ActionPointMenu, PuckMenu, MainMenu, NewObjectTypeMenu;
    GameObject MenuOpened;
    public GameObject ActionPointMenuPrefab, ButtonPrefab;

    
    public bool IsAnyMenuOpened() {
        return InteractiveObjectMenu.GetComponent<SimpleSideMenu>().CurrentState == SimpleSideMenu.State.Open ||
            ActionPointMenu.GetComponent<SimpleSideMenu>().CurrentState == SimpleSideMenu.State.Open ||
            PuckMenu.GetComponent<SimpleSideMenu>().CurrentState == SimpleSideMenu.State.Open ||
            MainMenu.GetComponent<SimpleSideMenu>().CurrentState == SimpleSideMenu.State.Open ||
            NewObjectTypeMenu.GetComponent<SimpleSideMenu>().CurrentState == SimpleSideMenu.State.Open;
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
    }

    public void HideMenu() {
        if (MenuOpened != null) {
            MenuOpened.SetActive(false);
        }

    }
}
