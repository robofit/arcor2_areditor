using System.Collections;
using System.Collections.Generic;
using System;
using UnityEngine;
using UnityEngine.UI;
using Michsky.UI.ModernUIPack;

public class OpenSceneDialog : Dialog {
    public GameObject ToggleGroup;
    public GameObject TogglePrefab;
    public override void Start() {
        base.Start();
        Base.GameManager.Instance.OnSceneListChanged += UpdateScenes;
    }

    public void UpdateScenes(object sender, EventArgs eventArgs) {
        UpdateToggleGroup(TogglePrefab, ToggleGroup, Base.GameManager.Instance.Scenes);
    }

    public async void OpenScene() {
        string sceneName;
        try {
            sceneName = GetSelectedValue(ToggleGroup);
            await Base.GameManager.Instance.OpenScene(sceneName);
            Close();
        } catch (Exception ex) when (ex is Base.ItemNotFoundException) {
            Base.NotificationsModernUI.Instance.ShowNotification("Failed to open scene", ex.Message);
        }
    }

    public override void Confirm() {
        OpenScene();
    }
}
