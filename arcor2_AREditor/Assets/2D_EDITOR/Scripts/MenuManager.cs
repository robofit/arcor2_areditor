using System.Collections.Generic;
using DanielLochner.Assets.SimpleSideMenu;
using UnityEngine;
using UnityEngine.UI;

public class MenuManager : Base.Singleton<MenuManager> {
    public SimpleSideMenu InteractiveObjectMenu, ActionPointMenu, PuckMenu, MainMenu, NewObjectTypeMenu, ActionObjectMenuProjectEditor;
    GameObject MenuOpened;
    public GameObject ActionPointMenuPrefab, ButtonPrefab;


    public bool IsAnyMenuOpened() {
        return InteractiveObjectMenu.CurrentState == SimpleSideMenu.State.Open ||
            ActionPointMenu.CurrentState == SimpleSideMenu.State.Open ||
            PuckMenu.CurrentState == SimpleSideMenu.State.Open ||
            MainMenu.CurrentState == SimpleSideMenu.State.Open ||
            NewObjectTypeMenu.CurrentState == SimpleSideMenu.State.Open ||
            ActionObjectMenuProjectEditor.CurrentState == SimpleSideMenu.State.Open;
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
        if (InteractiveObjectMenu.CurrentState == SimpleSideMenu.State.Open) {
            InteractiveObjectMenu.Close();
        }
        if (ActionObjectMenuProjectEditor.CurrentState == SimpleSideMenu.State.Open) {
            ActionObjectMenuProjectEditor.Close();
        }
        if (ActionPointMenu.CurrentState == SimpleSideMenu.State.Open) {
            ActionPointMenu.Close();
        }
        if (PuckMenu.CurrentState == SimpleSideMenu.State.Open) {
            PuckMenu.Close();
        }
    }

    public void DisableAllMenus() {
        MainMenu.gameObject.SetActive(false);
        InteractiveObjectMenu.gameObject.SetActive(false);
        ActionPointMenu.gameObject.SetActive(false);
        PuckMenu.gameObject.SetActive(false);
        ActionObjectMenuProjectEditor.gameObject.SetActive(false);
    }

    public void EnableAllWindows() {
        MainMenu.gameObject.SetActive(true);
        InteractiveObjectMenu.gameObject.SetActive(true);
        ActionPointMenu.gameObject.SetActive(true);
        PuckMenu.gameObject.SetActive(true);
        ActionObjectMenuProjectEditor.gameObject.SetActive(true);
    }

    public void HideMenu() {
        if (MenuOpened != null) {
            MenuOpened.SetActive(false);
        }
    }
}
