using System;
using Arcor2.ClientSdk.Communication;
using Base;
using TMPro;
using UnityEngine;

public class RobotInfoMenu : Singleton<RobotInfoMenu> {

    public TMP_Text SelectedRobot, SelectedArm, SelectedEE;

    [SerializeField]
    private CanvasGroup canvasGroup;

    private void Start() {
        SceneManager.Instance.OnRobotSelected += OnRobotSelected;
        SceneManager.Instance.OnSceneStateEvent += OnSceneStateEvent;
    }

    private void OnSceneStateEvent(object sender, SceneStateEventArgs args) {
        UpdateLabels();
    }

    private void OnRobotSelected(object sender, EventArgs e) {
        UpdateLabels();
    }

    private void UpdateLabels() {
        if (!SceneManager.Instance.SceneStarted) {
            SelectedRobot.text = "Robot is offline";
            SelectedEE.text = "";
            SelectedArm.text = "";
        } else if (SceneManager.Instance.IsRobotSelected()) {
            SelectedRobot.text = $"Robot: {SceneManager.Instance.SelectedRobot.GetName()}";
            SelectedArm.text = $"Arm: {SceneManager.Instance.SelectedArmId}";
            if (SceneManager.Instance.IsRobotAndEESelected()) {
                SelectedEE.text = $"End effector: {SceneManager.Instance.SelectedEndEffector.GetName()}";

            } else {
                SelectedEE.text = "End effector: not selected";
            }
        } else {
            SelectedRobot.text = "Robot: not selected";
            SelectedEE.text = "End effector: not selected";
            SelectedArm.text = "Arm: not selected";
        }
    }

    public void Show() {
        EditorHelper.EnableCanvasGroup(canvasGroup, true);
    }

    public void Hide() {
        EditorHelper.EnableCanvasGroup(canvasGroup, false);
    }
}
