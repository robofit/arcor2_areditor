using System.Collections;
using System.Collections.Generic;
using System;
using UnityEngine;
using UnityEngine.UI;
using Michsky.UI.ModernUIPack;
using System.Threading.Tasks;

public class NewProjectDialog : Dialog
{
    public GameObject ToggleGroup, GenerateLogicToggle;
    public GameObject TogglePrefab;
    public TMPro.TMP_InputField NewProjectName;
    public Button OKBtn;
    public TooltipContent TooltipContent;
    public override void Start()
    {
        base.Start();
        Base.GameManager.Instance.OnSceneListChanged += UpdateScenes;
        TooltipContent.descriptionText = TooltipRef.Instance.Text;
        TooltipContent.tooltipRect = TooltipRef.Instance.Tooltip;
    }

    public void UpdateScenes(object sender, EventArgs eventArgs) {
        UpdateToggleGroup(TogglePrefab, ToggleGroup, Base.GameManager.Instance.Scenes);
        FieldChanged();
    }

    public async void NewProject() {
        string name = NewProjectName.text;
        string sceneName;
        bool generateLogic;
        try {
            sceneName = GetSelectedValue(ToggleGroup);
            string sceneId = Base.GameManager.Instance.GetSceneId(sceneName);
            generateLogic = GenerateLogicToggle.GetComponent<Toggle>().isOn;
            await Base.GameManager.Instance.NewProject(name, sceneId, generateLogic);
            WindowManager.CloseWindow();
        } catch (Exception ex) when (ex is Base.ItemNotFoundException || ex is Base.RequestFailedException) { 
            Base.Notifications.Instance.ShowNotification("Failed to create new project", ex.Message);
        }



    }

    public async void FieldChanged() {
        OKBtn.interactable = await ValidateFields();

    }

    public async Task<bool> ValidateFields() {
        string name = NewProjectName.text;
        string sceneName;
        string sceneId;
        bool generateLogic = GenerateLogicToggle.GetComponent<Toggle>().isOn;
        if (string.IsNullOrEmpty(name)) {
            TooltipContent.description = "Name cannot be empty";
            return false;
        }
        try {
            sceneName = GetSelectedValue(ToggleGroup);
            sceneId = Base.GameManager.Instance.GetSceneId(sceneName);
            
        } catch (Base.ItemNotFoundException ex) {
            TooltipContent.description = "No scene selected";
            return false;
        }
        try {
            await Base.WebsocketManager.Instance.CreateProject(name, sceneId, "", generateLogic, true);
        } catch (Base.RequestFailedException ex) {
            TooltipContent.description = ex.Message;
            return false;
        }
        TooltipContent.description = "";
        return true;
    }

}
