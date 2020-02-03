using System.Collections;
using System.Collections.Generic;
using System;
using UnityEngine;
using UnityEngine.UI;
using Michsky.UI.ModernUIPack;

public class OpenProjectDialog : Dialog {
    public GameObject ToggleGroup;
    public GameObject TogglePrefab;
    private void Start() {
        Base.GameManager.Instance.OnProjectsListChanged += UpdateProjects;
    }

    public void UpdateProjects(object sender, EventArgs eventArgs) {
        UpdateToggleGroup(TogglePrefab, ToggleGroup, Base.GameManager.Instance.Projects);
    }

    public void OpenProject() {
        string projectName;
        try {
            projectName = GetSelectedValue(ToggleGroup);
            Base.GameManager.Instance.OpenProject(projectName);
            WindowManager.CloseWindow();
        } catch (Exception ex) when (ex is Base.ItemNotFoundException || ex is Base.RequestFailedException) {
            Base.NotificationsModernUI.Instance.ShowNotification("Failed to open project", ex.Message);
        }
    }
}
