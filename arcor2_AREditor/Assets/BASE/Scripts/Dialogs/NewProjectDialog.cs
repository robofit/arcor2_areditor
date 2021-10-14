using System.Collections;
using System.Collections.Generic;
using System;
using UnityEngine;
using UnityEngine.UI;
using Michsky.UI.ModernUIPack;
using System.Threading.Tasks;
using Base;

public class NewProjectDialog : Dialog
{
    public GameObject ToggleGroup, GenerateLogicToggle;
    public GameObject TogglePrefab;
    public LabeledInput NewProjectName;
    public ButtonWithTooltip OKBtn;
    public void Start()
    {
        Base.GameManager.Instance.OnScenesListChanged += UpdateScenes;
    }

    public void UpdateScenes(object sender, EventArgs eventArgs) {
        UpdateToggleGroup(TogglePrefab, ToggleGroup, Base.GameManager.Instance.Scenes);
        if (Visible)
            FieldChanged();
    }

    public async void NewProject() {
        string name = NewProjectName.GetValue()?.ToString();
        string sceneName;
        bool generateLogic;
        try {
            sceneName = GetSelectedValue(ToggleGroup);
            string sceneId = Base.GameManager.Instance.GetSceneId(sceneName);
            generateLogic = GenerateLogicToggle.GetComponent<Toggle>().isOn;
            await Base.GameManager.Instance.NewProject(name, sceneId, generateLogic);
            Close();
        } catch (Exception ex) when (ex is Base.ItemNotFoundException || ex is Base.RequestFailedException) { 
            Base.Notifications.Instance.ShowNotification("Failed to create new project", ex.Message);
        }



    }

    public async void FieldChanged() {
        Base.RequestResult result = await ValidateFields();
        OKBtn.SetInteractivity(result.Success, result.Message);

    }

    public async Task<Base.RequestResult> ValidateFields() {
        string name = NewProjectName.GetValue()?.ToString();
        string sceneName;
        string sceneId;
        bool generateLogic = GenerateLogicToggle.GetComponent<Toggle>().isOn;
        if (string.IsNullOrEmpty(name)) {
            return (false, "Name cannot be empty");
        }
        try {
            sceneName = GetSelectedValue(ToggleGroup);
            sceneId = Base.GameManager.Instance.GetSceneId(sceneName);
            
        } catch (Base.ItemNotFoundException ex) {
            return (false, "No scene selected");
        }
        try {
            await Base.WebsocketManager.Instance.CreateProject(name, sceneId, "", generateLogic, true);
        } catch (Base.RequestFailedException ex) {
            return (false, ex.Message);
        }
        return (true, "");
    }

    public async override void Confirm() {
        Base.RequestResult result = await ValidateFields();
        if (result.Success)
            NewProject();
        else {
            Notifications.Instance.ShowNotification("Failed to create new project", result.Message);
        }
    }

    public void Open(string selectedScene = null) {
        base.Open();
        if (selectedScene != null) {
            SetSelectedValue(ToggleGroup, selectedScene);
        }
        NewProjectName.SetValue("");
        FieldChanged();
    }
}
