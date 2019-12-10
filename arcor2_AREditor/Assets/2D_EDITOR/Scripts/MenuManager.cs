using System.Collections.Generic;
using DanielLochner.Assets.SimpleSideMenu;
using UnityEngine;
using UnityEngine.UI;

public class MenuManager : Base.Singleton<MenuManager> {
    public GameObject InteractiveObjectMenu, ActionPointMenu, PuckMenu, MainMenu, NewObjectTypeMenu, ActionObjectMenuProjectEditor;
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
        if (ActionObjectMenuProjectEditor.GetComponent<SimpleSideMenu>().CurrentState == SimpleSideMenu.State.Open) {
            ActionObjectMenuProjectEditor.GetComponent<SimpleSideMenu>().Close();
        }
        if (ActionPointMenu.GetComponent<SimpleSideMenu>().CurrentState == SimpleSideMenu.State.Open) {
            ActionPointMenu.GetComponent<SimpleSideMenu>().Close();
        }
        if (PuckMenu.GetComponent<SimpleSideMenu>().CurrentState == SimpleSideMenu.State.Open) {
            PuckMenu.GetComponent<SimpleSideMenu>().Close();
        }
    }

    public void DisableAllMenus() {
        MainMenu.SetActive(false);
        InteractiveObjectMenu.SetActive(false);
        ActionPointMenu.SetActive(false);
        PuckMenu.SetActive(false);
        ActionObjectMenuProjectEditor.SetActive(false);
    }

    public void EnableAllWindows() {
        MainMenu.SetActive(true);
        InteractiveObjectMenu.SetActive(true);
        ActionPointMenu.SetActive(true);
        PuckMenu.SetActive(true);
        ActionObjectMenuProjectEditor.SetActive(true);
    }

    public void HideMenu() {
        if (MenuOpened != null) {
            MenuOpened.SetActive(false);
        }

    }
}
