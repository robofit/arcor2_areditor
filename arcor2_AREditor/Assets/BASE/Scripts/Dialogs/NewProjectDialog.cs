using System;
using System.Linq;
using System.Threading.Tasks;
using Arcor2.ClientSdk.Communication;
using Arcor2.ClientSdk.Communication.OpenApi.Models;
using Base;
using UnityEngine;
using UnityEngine.UI;

public class NewProjectDialog : Dialog
{
    public GameObject ToggleGroup, GenerateLogicToggle;
    public GameObject TogglePrefab;
    public LabeledInput NewProjectName;
    public ButtonWithTooltip OKBtn;
    public void Start()
    {
        GameManager.Instance.OnScenesListChanged += UpdateScenes;
    }

    public void UpdateScenes(object sender, EventArgs eventArgs) {
        UpdateToggleGroup(TogglePrefab, ToggleGroup, GameManager.Instance.Scenes);
        if (Visible)
            FieldChanged();
    }

    public async void NewProject() {
        string name = NewProjectName.GetValue()?.ToString();
        string sceneName;
        bool generateLogic;
        try {
            sceneName = GetSelectedValue(ToggleGroup);
            string sceneId = GameManager.Instance.GetSceneId(sceneName);
            generateLogic = GenerateLogicToggle.GetComponent<Toggle>().isOn;
            await GameManager.Instance.NewProject(name, sceneId, generateLogic);
            Close();
        } catch (Exception ex) when (ex is ItemNotFoundException || ex is Arcor2ConnectionException) { 
            Notifications.Instance.ShowNotification("Failed to create new project", ex.Message);
        }



    }

    public async void FieldChanged() {
        RequestResult result = await ValidateFields();
        OKBtn.SetInteractivity(result.Success, result.Message);

    }

    public async Task<RequestResult> ValidateFields() {
        string name = NewProjectName.GetValue()?.ToString();
        string sceneName;
        string sceneId;
        bool generateLogic = GenerateLogicToggle.GetComponent<Toggle>().isOn;
        if (string.IsNullOrEmpty(name)) {
            return (false, "Name cannot be empty");
        }
        try {
            sceneName = GetSelectedValue(ToggleGroup);
            sceneId = GameManager.Instance.GetSceneId(sceneName);
            
        } catch (ItemNotFoundException ex) {
            return (false, "No scene selected");
        }
        try {
            var response = await CommunicationManager.Instance.Client.AddNewProjectAsync(new NewProjectRequestArgs(name, sceneId, "", generateLogic), true);
            return (response.Result, response.Messages?.FirstOrDefault() ?? "");
        } catch (Arcor2ConnectionException ex) {
            return (false, ex.Message);
        }
        return (true, "");
    }

    public async override void Confirm() {
        RequestResult result = await ValidateFields();
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
