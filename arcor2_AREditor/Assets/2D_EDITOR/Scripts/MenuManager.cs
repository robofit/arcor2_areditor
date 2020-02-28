using System.Collections.Generic;
using DanielLochner.Assets.SimpleSideMenu;
using UnityEngine;
using UnityEngine.UI;

public class MenuManager : Base.Singleton<MenuManager> {
    public SimpleSideMenu ActionObjectMenuSceneEditor, ActionPointMenu, PuckMenu, MainMenu, NewObjectTypeMenu, ActionObjectMenuProjectEditor;
    SimpleSideMenu MenuOpened;
    public GameObject ActionPointMenuPrefab, ButtonPrefab;


    public bool IsAnyMenuOpened() {
        return ActionObjectMenuSceneEditor.CurrentState == SimpleSideMenu.State.Open ||
            ActionPointMenu.CurrentState == SimpleSideMenu.State.Open ||
            PuckMenu.CurrentState == SimpleSideMenu.State.Open ||
            MainMenu.CurrentState == SimpleSideMenu.State.Open ||
            NewObjectTypeMenu.CurrentState == SimpleSideMenu.State.Open ||
            ActionObjectMenuProjectEditor.CurrentState == SimpleSideMenu.State.Open;
    }

    public void ShowMenu(SimpleSideMenu menu) {
        //Debug.Log(Menu); 
        if (menu == null)
            return;
        HideAllMenus();
        menu.Open();
        menu.gameObject.GetComponent<IMenu>().UpdateMenu();
        MenuOpened = menu;
    }

    public void HideAllMenus() {
        if (ActionObjectMenuSceneEditor.CurrentState == SimpleSideMenu.State.Open) {
            ActionObjectMenuSceneEditor.Close();
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
        ActionObjectMenuSceneEditor.gameObject.SetActive(false);
        ActionPointMenu.gameObject.SetActive(false);
        PuckMenu.gameObject.SetActive(false);
        ActionObjectMenuProjectEditor.gameObject.SetActive(false);
    }

    public void EnableAllWindows() {
        MainMenu.gameObject.SetActive(true);
        ActionObjectMenuSceneEditor.gameObject.SetActive(true);
        ActionPointMenu.gameObject.SetActive(true);
        PuckMenu.gameObject.SetActive(true);
        ActionObjectMenuProjectEditor.gameObject.SetActive(true);
    }

    public void HideMenu(SimpleSideMenu menu) {
        if (MenuOpened != null) {
            MenuOpened.Close();
            MenuOpened = null;
        }
    }
}
