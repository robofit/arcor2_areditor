using System.Collections;
using System.Collections.Generic;
using System;
using UnityEngine;
using UnityEngine.UI;
using Michsky.UI.ModernUIPack;

public class NewProjectDialog : MonoBehaviour
{
    public GameObject Dropdown, NewProjectName;
    void Start()
    {
        Base.GameManager.Instance.OnSceneListChanged += UpdateScenes;

    }

    public void UpdateScenes(object sender, EventArgs eventArgs) {
        
        Debug.LogError("scenes udpate");
        Dropdown.GetComponent<CustomDropdown>().dropdownItems.Clear();
        Dropdown.GetComponent<CustomDropdown>().SetItemTitle("Create new scene");
        Dropdown.GetComponent<CustomDropdown>().CreateNewItem();

        foreach (IO.Swagger.Model.IdDesc scene in Base.GameManager.Instance.Scenes) {
            Dropdown.GetComponent<CustomDropdown>().SetItemTitle(scene.Id);
            Dropdown.GetComponent<CustomDropdown>().CreateNewItem();
        }
         
    }

    public void NewProject() {
        string name = NewProjectName.GetComponent<TMPro.TMP_InputField>().text;
        string scene = Dropdown.GetComponent<CustomDropdown>().selectedText.text;
        if (Dropdown.GetComponent<CustomDropdown>().selectedItemIndex == 0) {
            scene = null;
        }        
        Base.GameManager.Instance.NewProject(name, scene);
    }
}
