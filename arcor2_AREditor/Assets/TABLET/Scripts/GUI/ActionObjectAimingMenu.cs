using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Base;
using UnityEngine;
using UnityEngine.UI;
using static IO.Swagger.Model.UpdateObjectPoseUsingRobotRequestArgs;

public class ActionObjectAimingMenu : Base.Singleton<ActionObjectAimingMenu>
{
    public DropdownParameter RobotsList, EndEffectorList, PivotList;
    public DropdownArms DropdownArms;
    public Button NextButton, PreviousButton, FocusObjectDoneButton, StartObjectFocusingButton, SavePositionButton;
    public TMPro.TMP_Text CurrentPointLabel;
    public GameObject RobotsListsBlock, UpdatePositionBlockMesh, UpdatePositionBlockVO;
    public SwitchComponent ShowModelSwitch;
    private int currentFocusPoint = -1;
    public CalibrateRobotDialog CalibrateRobotDialog;
    private GameObject model;
    public ButtonWithTooltip CalibrateBtn;

    private ActionObject currentObject;

    public ConfirmationDialog ConfirmationDialog;

    public CanvasGroup CanvasGroup;

    public bool Focusing;



    private void Start() {
        Debug.Assert(RobotsList != null);
        Debug.Assert(EndEffectorList != null);
        Debug.Assert(NextButton != null);
        Debug.Assert(PreviousButton != null);
        Debug.Assert(FocusObjectDoneButton != null);
        Debug.Assert(StartObjectFocusingButton != null);
        Debug.Assert(SavePositionButton != null);
        Debug.Assert(CurrentPointLabel != null);
        Debug.Assert(RobotsListsBlock != null);
        Debug.Assert(UpdatePositionBlockMesh != null);
        Debug.Assert(UpdatePositionBlockVO != null);
        List<string> pivots = new List<string>();
        foreach (string item in Enum.GetNames(typeof(PivotEnum))) {
            pivots.Add(item);
        }
        PivotList.PutData(pivots, "Middle", OnPivotChanged);
        Focusing = false;
        WebsocketManager.Instance.OnProcessStateEvent += OnRobotCalibrationEvent;
    }

    public async void Show(ActionObject actionObject) {
        if (!await actionObject.WriteLock(false))
            return;
        currentObject = actionObject;
        UpdateMenu();
        EditorHelper.EnableCanvasGroup(CanvasGroup, true);
    }

    public async void Hide(bool unlock = true) {
        HideModelOnEE();
        EditorHelper.EnableCanvasGroup(CanvasGroup, false);
        if (currentObject != null) {
            if (unlock)
                await currentObject.WriteUnlock();
            currentObject = null;
        }
    }

    private void OnRobotCalibrationEvent(object sender, ProcessStateEventArgs args) {
        GameManager.Instance.HideLoadingScreen();
        if (args.Data.State == IO.Swagger.Model.ProcessStateData.StateEnum.Finished) {
            Notifications.Instance.ShowToastMessage("Calibration finished successfuly");
        } else if (args.Data.State == IO.Swagger.Model.ProcessStateData.StateEnum.Failed) {
            Notifications.Instance.ShowNotification("Calibration failed", args.Data.Message);
        }
    }

    public async void UpdateMenu() {
        CalibrateBtn.gameObject.SetActive(false);


        if (!SceneManager.Instance.SceneStarted) {
            UpdatePositionBlockVO.SetActive(false);
            UpdatePositionBlockMesh.SetActive(false);
            RobotsListsBlock.SetActive(false);
            Hide();
            return;
        }


        CalibrateBtn.Button.onClick.RemoveAllListeners();
        if (currentObject.IsRobot()) {
            CalibrateBtn.gameObject.SetActive(true);
            CalibrateBtn.SetDescription("Calibrate robot");
            CalibrateBtn.Button.onClick.AddListener(() => ShowCalibrateRobotDialog());
        } else if (currentObject.IsCamera()) {
            CalibrateBtn.gameObject.SetActive(true);
            CalibrateBtn.SetDescription("Calibrate camera");
            CalibrateBtn.Button.onClick.AddListener(() => ShowCalibrateCameraDialog());
        }

        if (currentFocusPoint >= 0)
            return;
        if (SceneManager.Instance.RobotInScene()) {
            await RobotsList.gameObject.GetComponent<DropdownRobots>().Init(OnRobotChanged, true);
            OnRobotChanged(RobotsList.GetValue().ToString());

            if (currentObject.ActionObjectMetadata.ObjectModel?.Type == IO.Swagger.Model.ObjectModel.TypeEnum.Mesh) {
                UpdatePositionBlockVO.SetActive(false);
                UpdatePositionBlockMesh.SetActive(true);
                RobotsListsBlock.SetActive(true);
            } else if (currentObject.ActionObjectMetadata.ObjectModel != null) {
                UpdatePositionBlockVO.SetActive(true);
                UpdatePositionBlockMesh.SetActive(false);
                RobotsListsBlock.SetActive(true);
                ShowModelSwitch.Interactable = SceneManager.Instance.RobotsEEVisible;
                if (ShowModelSwitch.Interactable && ShowModelSwitch.Switch.isOn) {
                    ShowModelOnEE();
                }
            } else {
                UpdatePositionBlockVO.SetActive(false);
                UpdatePositionBlockMesh.SetActive(false);
                RobotsListsBlock.SetActive(false);
            }

        } else {
            UpdatePositionBlockVO.SetActive(false);
            UpdatePositionBlockMesh.SetActive(false);
            RobotsListsBlock.SetActive(false);
        }


        FocusObjectDoneButton.interactable = false;
        NextButton.interactable = false;
        PreviousButton.interactable = false;


    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="robot_name"></param>
    private async void OnRobotChanged(string robot_name) {
        string robotId = null;
        try {
            robotId = SceneManager.Instance.RobotNameToId(RobotsList.GetValue().ToString());
        } catch (ItemNotFoundException ex) {
            Debug.LogError(ex);
            robotId = null;
        }
        if (string.IsNullOrEmpty(robotId)) {
            Notifications.Instance.ShowNotification("Robot not found", "Robot with name " + RobotsList.GetValue().ToString() + "does not exists");
            return;
        }
        await DropdownArms.Init(robotId, OnRobotArmChanged);
        OnRobotArmChanged(DropdownArms.Dropdown.GetValue().ToString());
    }

    private async void OnRobotArmChanged(string arm_id) {
        string robotId = null;
        try {
            robotId = SceneManager.Instance.RobotNameToId(RobotsList.GetValue().ToString());
        } catch (ItemNotFoundException ex) {
            Debug.LogError(ex);
            robotId = null;

        }
        if (string.IsNullOrEmpty(robotId)) {
            Notifications.Instance.ShowNotification("Robot not found", "Robot with name " + RobotsList.GetValue().ToString() + "does not exists");
            return;
        }
        await EndEffectorList.gameObject.GetComponent<DropdownEndEffectors>().Init(robotId, arm_id, OnEEChanged);
        UpdateModelOnEE();
    }



    private void OnEEChanged(string eeId) {
        UpdateModelOnEE();
    }

    private void OnPivotChanged(string pivot) {
        UpdateModelOnEE();
    }


    public async void UpdateObjectPosition() {
        if (RobotsList.Dropdown.dropdownItems.Count == 0 || EndEffectorList.Dropdown.dropdownItems.Count == 0) {
            Base.NotificationsModernUI.Instance.ShowNotification("Failed to update object position", "No robot or end effector available");
            return;
        }
        PivotEnum pivot = (PivotEnum) Enum.Parse(typeof(PivotEnum), (string) PivotList.GetValue());
        IRobot robot = SceneManager.Instance.GetRobotByName((string) RobotsList.GetValue());
        string armId = null;
        if (robot.MultiArm())
            armId = DropdownArms.Dropdown.GetValue().ToString();
        try {
            await WebsocketManager.Instance.UpdateActionObjectPoseUsingRobot(currentObject.Data.Id,
                    robot.GetId(), (string) EndEffectorList.GetValue(), pivot, armId);
        } catch (RequestFailedException ex) {
            Notifications.Instance.ShowNotification("Failed to update object position", ex.Message);
        }

    }


    private void EnableFocusControls() {
        SavePositionButton.GetComponent<Button>().interactable = true;
        StartObjectFocusingButton.GetComponent<Button>().interactable = true;
        NextButton.GetComponent<Button>().interactable = true;
        PreviousButton.GetComponent<Button>().interactable = true;
        FocusObjectDoneButton.GetComponent<Button>().interactable = true;
    }

    private void DisableFocusControls() {
        SavePositionButton.GetComponent<Button>().interactable = false;
        StartObjectFocusingButton.GetComponent<Button>().interactable = false;
        NextButton.GetComponent<Button>().interactable = false;
        PreviousButton.GetComponent<Button>().interactable = false;
        FocusObjectDoneButton.GetComponent<Button>().interactable = false;
    }

    public async void StartObjectFocusing() {
        if (RobotsList.Dropdown.dropdownItems.Count == 0 || EndEffectorList.Dropdown.dropdownItems.Count == 0) {
            Base.NotificationsModernUI.Instance.ShowNotification("Failed to update object position", "No robot or end effector available");
            return;
        }
        try {
            Focusing = true;
            IRobot robot = SceneManager.Instance.GetRobot((string) RobotsList.GetValue());
            string armId = null;
            if (robot.MultiArm())
                armId = DropdownArms.Dropdown.GetValue().ToString();
            await WebsocketManager.Instance.StartObjectFocusing(currentObject.Data.Id,
                (string) RobotsList.GetValue(),
                (string) EndEffectorList.GetValue(),
                armId);
            currentFocusPoint = 0;
            UpdateCurrentPointLabel();
            //TODO: ZAJISTIT ABY MENU NEŠLO ZAVŘÍT když běží focusing - ideálně nějaký dialog
            //GetComponent<SimpleSideMenu>().handleToggleStateOnPressed = false;
            //GetComponent<SimpleSideMenu>().overlayCloseOnPressed = false;

            FocusObjectDoneButton.interactable = true;
            if (currentObject.ActionObjectMetadata.ObjectModel.Mesh.FocusPoints.Count > 1) {
                NextButton.interactable = true;
                PreviousButton.interactable = true;
            }
        } catch (Base.RequestFailedException ex) {
            Base.NotificationsModernUI.Instance.ShowNotification("Failed to start object focusing", ex.Message);
            CurrentPointLabel.text = "";
            currentFocusPoint = -1;
            Focusing = false;
            if (ex.Message == "Focusing already started.") { //TODO HACK! find better solution
                FocusObjectDone();
            }
        }
    }

    public async void SavePosition() {
        if (currentFocusPoint < 0)
            return;
        try {
            await WebsocketManager.Instance.SavePosition(currentObject.Data.Id, currentFocusPoint);
        } catch (Base.RequestFailedException ex) {
            Base.NotificationsModernUI.Instance.ShowNotification("Failed to save current position", ex.Message);
        }


    }

    public async void FocusObjectDone() {
        try {
            CurrentPointLabel.text = "";
            
            // TODO: znovupovolit zavření menu
            //GetComponent<SimpleSideMenu>().handleToggleStateOnPressed = true;
            //GetComponent<SimpleSideMenu>().overlayCloseOnPressed = true;
            currentFocusPoint = -1;
            FocusObjectDoneButton.interactable = false;
            NextButton.interactable = false;
            PreviousButton.interactable = false;
            await WebsocketManager.Instance.FocusObjectDone(currentObject.Data.Id);
            Focusing = false;
        } catch (Base.RequestFailedException ex) {
            Base.NotificationsModernUI.Instance.ShowNotification("Failed to focus object", ex.Message);
        }
    }

    public void NextPoint() {
        currentFocusPoint = Math.Min(currentFocusPoint + 1, currentObject.ActionObjectMetadata.ObjectModel.Mesh.FocusPoints.Count - 1);
        PreviousButton.interactable = true;
        if (currentFocusPoint == currentObject.ActionObjectMetadata.ObjectModel.Mesh.FocusPoints.Count - 1) {
            NextButton.GetComponent<Button>().interactable = false;
        } else {
            NextButton.GetComponent<Button>().interactable = true;
        }
        UpdateCurrentPointLabel();
    }

    public void PreviousPoint() {
        currentFocusPoint = Math.Max(currentFocusPoint - 1, 0);
        NextButton.interactable = true;
        if (currentFocusPoint == 0) {
            PreviousButton.GetComponent<Button>().interactable = false;
        } else {
            PreviousButton.GetComponent<Button>().interactable = true;
        }
        UpdateCurrentPointLabel();
    }

    private void UpdateCurrentPointLabel() {
        CurrentPointLabel.text = "Point " + (currentFocusPoint + 1) + " out of " + currentObject.ActionObjectMetadata.ObjectModel.Mesh.FocusPoints.Count.ToString();
    }

    public void ShowModelOnEE() {
        if (model != null)
            HideModelOnEE();
        model = currentObject.GetModelCopy();
        if (model == null)
            return;
        UpdateModelOnEE();
    }

    private async void UpdateModelOnEE() {
        if (model == null)
            return;
        string robotName = (string) RobotsList.GetValue(), eeId = (string) EndEffectorList.GetValue();
        if (string.IsNullOrEmpty(robotName) || string.IsNullOrEmpty(eeId)) {
            throw new RequestFailedException("Robot or end effector not selected!");
        }

        try {
            string robotId = SceneManager.Instance.RobotNameToId(robotName);
            RobotEE ee = await (SceneManager.Instance.GetRobot(robotId).GetEE(eeId));
            model.transform.parent = ee.gameObject.transform;

            switch ((PivotEnum) Enum.Parse(typeof(PivotEnum), (string) PivotList.GetValue())) {
                case PivotEnum.Top:
                    model.transform.localPosition = new Vector3(0, model.transform.localScale.y / 2, 0);
                    break;
                case PivotEnum.Bottom:
                    model.transform.localPosition = new Vector3(0, -model.transform.localScale.y / 2, 0);
                    break;
                case PivotEnum.Middle:
                    model.transform.localPosition = new Vector3(0, 0, 0);
                    break;
            }
            model.transform.localRotation = new Quaternion(0, 0, 0, 1);
        } catch (ItemNotFoundException ex) {
            Debug.LogError(ex);
            Notifications.Instance.ShowNotification("End-effector position unknown", "Robot did not send position of selected end effector");
            ShowModelSwitch.Switch.isOn = false;
        }

    }

    public void HideModelOnEE() {
        if (model != null) {
            Destroy(model);
        }
        model = null;
    }





    public void ShowCalibrateRobotDialog() {
        CalibrateRobotDialog.Init(SceneManager.Instance.GetCamerasNames(), currentObject.Data.Id);
        CalibrateRobotDialog.Open();
    }


    public void ShowCalibrateCameraDialog() {
        ConfirmationDialog.Open("Camera calibration", "Are you sure you want to initiate camera calibration?",
            async () => await CalibrateCamera(), () => ConfirmationDialog.Close());
    }

    public async Task CalibrateCamera() {
        try {
            GameManager.Instance.ShowLoadingScreen("Calibrating camera...");
            await WebsocketManager.Instance.CalibrateCamera(currentObject.Data.Id);
        } catch (RequestFailedException ex) {
            GameManager.Instance.HideLoadingScreen();
            Notifications.Instance.ShowNotification("Failed to calibrate camera", ex.Message);
            ConfirmationDialog.Close();
        }
    }

}
