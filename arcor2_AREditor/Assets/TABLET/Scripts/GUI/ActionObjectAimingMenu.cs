using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Base;
using UnityEngine;
using UnityEngine.UI;
using static IO.Swagger.Model.UpdateObjectPoseUsingRobotRequestArgs;

public class ActionObjectAimingMenu : Base.Singleton<ActionObjectAimingMenu>
{
    public DropdownParameter PivotList;
    public Button NextButton, PreviousButton, FocusObjectDoneButton, StartObjectFocusingButton, SavePositionButton;
    public TMPro.TMP_Text CurrentPointLabel;
    public GameObject UpdatePositionBlockMesh, UpdatePositionBlockVO;
    public SwitchComponent ShowModelSwitch;
    private int currentFocusPoint = -1;
    public CalibrateRobotDialog CalibrateRobotDialog;
    private GameObject model;
    public ButtonWithTooltip CalibrateBtn;

    private ActionObject currentObject;

    public ConfirmationDialog ConfirmationDialog;

    public CanvasGroup CanvasGroup;

    public bool Focusing;

    public GameObject Sphere;

    private List<GameObject> spheres = new List<GameObject>();



    private void Start() {
        Debug.Assert(NextButton != null);
        Debug.Assert(PreviousButton != null);
        Debug.Assert(FocusObjectDoneButton != null);
        Debug.Assert(StartObjectFocusingButton != null);
        Debug.Assert(SavePositionButton != null);
        Debug.Assert(CurrentPointLabel != null);
        Debug.Assert(UpdatePositionBlockMesh != null);
        Debug.Assert(UpdatePositionBlockVO != null);
        List<string> pivots = new List<string>();
        foreach (string item in Enum.GetNames(typeof(PivotEnum))) {
            pivots.Add(item);
        }
        PivotList.PutData(pivots, "Middle", OnPivotChanged);
        Focusing = false;
        WebsocketManager.Instance.OnProcessStateEvent += OnRobotCalibrationEvent;
        SceneManager.Instance.OnSceneStateEvent += OnSceneStateEvent;
    }

    private void OnSceneStateEvent(object sender, SceneStateEventArgs args) {
        Hide(false);
        
    }

    public async void Show(ActionObject actionObject) {
        if (!await actionObject.WriteLock(false) || !await SceneManager.Instance.SelectedRobot.WriteLock(false))
            return;
        currentObject = actionObject;
        UpdateMenu();
        EditorHelper.EnableCanvasGroup(CanvasGroup, true);
    }

    public async void Hide(bool unlock = true) {
        HideModelOnEE();
        EditorHelper.EnableCanvasGroup(CanvasGroup, false);
        foreach (GameObject sphere in spheres) {
            if (sphere != null) {
                Destroy(sphere.gameObject);
            }
        }
        spheres.Clear();
        if (currentObject != null) {
            if (unlock) {
                await currentObject.WriteUnlock();
                await SceneManager.Instance.SelectedRobot.WriteUnlock();
            }
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

            if (currentObject.ActionObjectMetadata.ObjectModel?.Type == IO.Swagger.Model.ObjectModel.TypeEnum.Mesh) {
                UpdatePositionBlockVO.SetActive(false);
                UpdatePositionBlockMesh.SetActive(true);
                foreach (IO.Swagger.Model.Pose point in currentObject.ActionObjectMetadata.ObjectModel.Mesh.FocusPoints) {
                    GameObject sphere = Instantiate(Sphere, currentObject.transform);
                    sphere.transform.localScale = new Vector3(0.02f, 0.02f, 0.02f);
                    sphere.transform.localPosition = DataHelper.PositionToVector3(point.Position);
                    sphere.transform.localRotation = DataHelper.OrientationToQuaternion(point.Orientation);
                    spheres.Add(sphere);
                }
            } else if (currentObject.ActionObjectMetadata.ObjectModel != null) {
                UpdatePositionBlockVO.SetActive(true);
                UpdatePositionBlockMesh.SetActive(false);
                ShowModelSwitch.Interactable = SceneManager.Instance.RobotsEEVisible;
                if (ShowModelSwitch.Interactable && ShowModelSwitch.Switch.isOn) {
                    ShowModelOnEE();
                }
            } else {
                UpdatePositionBlockVO.SetActive(false);
                UpdatePositionBlockMesh.SetActive(false);
            }

        } else {
            UpdatePositionBlockVO.SetActive(false);
            UpdatePositionBlockMesh.SetActive(false);
        }


        FocusObjectDoneButton.interactable = false;
        NextButton.interactable = false;
        PreviousButton.interactable = false;


    }

    private void OnEEChanged(string eeId) {
        UpdateModelOnEE();
    }

    private void OnPivotChanged(string pivot) {
        UpdateModelOnEE();
    }


    public async void UpdateObjectPosition() {
        if (!SceneManager.Instance.IsRobotAndEESelected()) {
            Base.NotificationsModernUI.Instance.ShowNotification("Failed to update object position", "No robot or end effector available");
            return;
        }
        PivotEnum pivot = (PivotEnum) Enum.Parse(typeof(PivotEnum), (string) PivotList.GetValue());
        string armId = null;
        if (SceneManager.Instance.SelectedRobot.MultiArm())
            armId = SceneManager.Instance.SelectedArmId;
        try {
            await WebsocketManager.Instance.UpdateActionObjectPoseUsingRobot(currentObject.Data.Id,
                    SceneManager.Instance.SelectedRobot.GetId(), SceneManager.Instance.SelectedEndEffector.GetName(), pivot, armId);
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
        if (!SceneManager.Instance.IsRobotAndEESelected()) {
            Base.NotificationsModernUI.Instance.ShowNotification("Failed to update object position", "No robot or end effector available");
            return;
        }
        try {
            Focusing = true;
            string armId = null;
            if (SceneManager.Instance.SelectedRobot.MultiArm())
                armId = SceneManager.Instance.SelectedArmId;
            await WebsocketManager.Instance.ObjectAimingStart(currentObject.Data.Id,
                SceneManager.Instance.SelectedRobot.GetId(),
                SceneManager.Instance.SelectedEndEffector.GetName(),
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
            await WebsocketManager.Instance.ObjectAimingAddPoint(currentFocusPoint);
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
            await WebsocketManager.Instance.ObjectAimingDone();
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

    private void UpdateModelOnEE() {
        if (model == null)
            return;
        if (!SceneManager.Instance.IsRobotAndEESelected()) {
            throw new RequestFailedException("Robot or end effector not selected!");
        }

        try {
            model.transform.parent = SceneManager.Instance.SelectedEndEffector.gameObject.transform;

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
