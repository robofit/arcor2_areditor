using System.Collections;
using System.Collections.Generic;
using System;
using UnityEngine;
using UnityEngine.UI;
using Michsky.UI.ModernUIPack;

public class NewProjectDialog : Dialog
{
    public GameObject ToggleGroup, GenerateLogicToggle;
    public GameObject TogglePrefab;
    public TMPro.TMP_InputField NewProjectName;
    private void Start()
    {
        Base.GameManager.Instance.OnSceneListChanged += UpdateScenes;

    }

    public void UpdateScenes(object sender, EventArgs eventArgs) {
        UpdateToggleGroup(TogglePrefab, ToggleGroup, Base.GameManager.Instance.Scenes);
    }

    public void NewProject() {
        string name = NewProjectName.text;
        string sceneName;
        bool generateLogic;
        try {
            sceneName = GetSelectedValue(ToggleGroup);
            generateLogic = GenerateLogicToggle.GetComponent<Toggle>().isOn;
            Base.GameManager.Instance.NewProject(name, sceneName, generateLogic);
            WindowManager.CloseWindow();
        } catch (Exception ex) when (ex is Base.ItemNotFoundException || ex is Base.RequestFailedException) { 
            Base.NotificationsModernUI.Instance.ShowNotification("Failed to create new project", ex.Message);
        }



    }
}
