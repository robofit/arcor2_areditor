using System.Collections;
using System.Collections.Generic;
using DanielLochner.Assets.SimpleSideMenu;
using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(SimpleSideMenu))]
public class CloseMenusWhenClickedOnOverlay : MonoBehaviour
{
    [SerializeField]
    private List<SimpleSideMenu> menus;
    private Button overlayBtn;

    private void Update() {
        //to be executed after start
        if (overlayBtn != null) {
            enabled = false;
            return;
        }
        if (GetComponent<SimpleSideMenu>().useOverlay) {
            overlayBtn = GameObject.Find(gameObject.name + " (Overlay)").GetComponent<Button>();
            overlayBtn.onClick.AddListener(CloseAllMenus);
        }
        
    }

    public void CloseAllMenus() {
        foreach (SimpleSideMenu menu in menus) {
            menu.Close();
        }
    }
}
