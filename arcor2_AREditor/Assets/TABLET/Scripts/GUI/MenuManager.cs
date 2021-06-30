using System;
using System.Collections.Generic;
using Base;
using DanielLochner.Assets.SimpleSideMenu;
using UnityEngine;
using UnityEngine.UI;

public class MenuManager : Base.Singleton<MenuManager> {
    public SimpleSideMenu ActionObjectMenuSceneEditor, ActionPointMenu, MainMenu, NewObjectTypeMenu,
        ActionObjectMenuProjectEditor, NotificationMenu;
    SimpleSideMenu MenuOpened;
    public GameObject ActionPointMenuPrefab, ButtonPrefab;

    public OutputTypeDialog OutputTypeDialog;
    public ConnectionSelectorDialog ConnectionSelectorDialog;
    public Dialog InputDialog, ConfirmationDialog, InputDialogWithToggle, EditConstantDialog;

    public LeftMenuProject LeftMenuProject;
    public ProjectConstantPicker ProjectConstantPicker;


    private void Start() {
        GameManager.Instance.OnCloseProject += OnCloseSceneOrProject;
        GameManager.Instance.OnCloseScene += OnCloseSceneOrProject;
    }

    private void OnCloseSceneOrProject(object sender, EventArgs e) {
        HideAllMenus();
    }

    public bool IsAnyMenuOpened {
        get;
        private set;
    } = false;

    private bool CheckIsAnyMenuOpened() {
        return ActionObjectMenuSceneEditor.CurrentState == SimpleSideMenu.State.Open ||
            ActionPointMenu.CurrentState == SimpleSideMenu.State.Open ||
            MainMenu.CurrentState == SimpleSideMenu.State.Open ||
            NewObjectTypeMenu.CurrentState == SimpleSideMenu.State.Open ||
            ActionObjectMenuProjectEditor.CurrentState == SimpleSideMenu.State.Open ||
            NotificationMenu.CurrentState == SimpleSideMenu.State.Open;
    }
    public bool CheckIsAnyRightMenuOpened() {
        return ActionObjectMenuSceneEditor.CurrentState == SimpleSideMenu.State.Open ||
            ActionPointMenu.CurrentState == SimpleSideMenu.State.Open ||
            NewObjectTypeMenu.CurrentState == SimpleSideMenu.State.Open ||
            ActionObjectMenuProjectEditor.CurrentState == SimpleSideMenu.State.Open ||
            NotificationMenu.CurrentState == SimpleSideMenu.State.Open;
    }


    public void ShowMenu(SimpleSideMenu menu) {
        Debug.Assert(menu != null); 
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
        if (MainMenu.CurrentState == SimpleSideMenu.State.Open) {
            MainMenu.Close();
        }
        ConfirmationDialog.Close();
        InputDialog.Close();
        InputDialogWithToggle.Close();
        EditConstantDialog.Close();
    }

    public void DisableAllMenus() {
        MainMenu.gameObject.SetActive(false);
        ActionObjectMenuSceneEditor.gameObject.SetActive(false);
        ActionPointMenu.gameObject.SetActive(false);
        ActionObjectMenuProjectEditor.gameObject.SetActive(false);
    }

    public void EnableAllWindows() {
        MainMenu.gameObject.SetActive(true);
        ActionObjectMenuSceneEditor.gameObject.SetActive(true);
        ActionPointMenu.gameObject.SetActive(true);
        ActionObjectMenuProjectEditor.gameObject.SetActive(true);
    }

    public void HideMenu() {
        if (MenuOpened != null) {
            MenuOpened.Close();
            MenuOpened = null;
        }
    }

    public void OnMenuStateChanged(SimpleSideMenu menu) {
        switch (menu.CurrentState) {
            case SimpleSideMenu.State.Open:
                IsAnyMenuOpened = true;
                GameManager.Instance.InvokeSceneInteractable(false);
                break;
            case SimpleSideMenu.State.Closed:
                if (!CheckIsAnyMenuOpened()) {
                    IsAnyMenuOpened = false;
                    // no menus are opened, scene should be interactable
                    // invoke an event from GameManager to let everyone know, that scene is interactable
                    GameManager.Instance.InvokeSceneInteractable(true);
                }

                if (menu == ActionPointMenu) {
                    menu.GetComponent<ActionPointMenu>().HideMenu();
                } else if (menu == ActionObjectMenuSceneEditor) {
                    menu.GetComponent<ActionObjectMenuSceneEditor>().HideMenu();
                } else if (menu == ActionObjectMenuProjectEditor) {
                    menu.GetComponent<ActionObjectMenuProjectEditor>().HideMenu();
                }

                break;
        }
    }
}
