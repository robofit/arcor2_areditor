using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System;

public class NewObjectTypeMenu : Base.Singleton<NewObjectTypeMenu> {
    [SerializeField]
    private GameObject BoxMenu, CylinderMenu, SphereMenu, STLMenu, NameInput, ParentsList, ModelsList;
    private Dictionary<string, GameObject> ModelMenus;

    private void Awake() {
        ModelMenus = new Dictionary<string, GameObject>() {
            { "None", null },
            { "Box", BoxMenu },
            { "Sphere", SphereMenu },
            { "Cylinder", CylinderMenu },
            { "STL", STLMenu },
            
        };
    }
    // Start is called before the first frame update
    void Start() {
        ActionsManager.Instance.OnActionObjectUpdate += UpdateObjectsList;
    }

    // Update is called once per frame
    void Update() {

    }

    public void UpdateMenu() {

    }

    public void UpdateModelsMenu(int value) {
        string modelType = ModelsList.GetComponent<Dropdown>().options[value].text;
        if (ModelMenus.TryGetValue(modelType, out GameObject menu)) {
            ShowModelMenu(menu);
        }
    }

    private void ShowModelMenu(GameObject menu) {
        BoxMenu.SetActive(false);
        CylinderMenu.SetActive(false);
        SphereMenu.SetActive(false);
        STLMenu.SetActive(false);
        menu?.SetActive(true);
    }

    public void UpdateObjectsList(object sender, EventArgs eventArgs) {
        string originalValue = "";
        if (ParentsList.GetComponent<Dropdown>().options.Count > 0) {
            originalValue = ParentsList.GetComponent<Dropdown>().options[ParentsList.GetComponent<Dropdown>().value].text;
            ParentsList.GetComponent<Dropdown>().options.Clear();
        }         
        foreach (Base.ActionObjectMetadata actionObjectMetadata in ActionsManager.Instance.ActionObjectMetadata.Values) {
            ParentsList.GetComponent<Dropdown>().options.Add(new Dropdown.OptionData(actionObjectMetadata.Type));
        }
        if (originalValue != "") {
            // TODO: try if indexof works!
            int index = ParentsList.GetComponent<Dropdown>().options.IndexOf(new Dropdown.OptionData(originalValue));
            if (index >= 0) {
                ParentsList.GetComponent<Dropdown>().value = index;
            }
        }
        try {
            ParentsList.GetComponent<Dropdown>().captionText.text = ParentsList.GetComponent<Dropdown>().options[ParentsList.GetComponent<Dropdown>().value].text;
        } catch (KeyNotFoundException) {

        }
        
    }

}
