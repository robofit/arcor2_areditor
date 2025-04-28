using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Arcor2.ClientSdk.Communication;
using Arcor2.ClientSdk.Communication.OpenApi.Models;
using Base;
using TMPro;
using UnityEngine;
using Pose = Arcor2.ClientSdk.Communication.OpenApi.Models.Pose;

public class ActionObjectAimingMenu : RightMenu<ActionObjectAimingMenu> {
    public DropdownParameter PivotList;
    public ButtonWithTooltip NextButton, PreviousButton, FocusObjectDoneButton, StartObjectFocusingButton, SavePositionButton, CancelAimingButton;
    public TMP_Text CurrentPointLabel;
    public GameObject UpdatePositionBlockMesh, UpdatePositionBlockVO;
    public SwitchComponent ShowModelSwitch;
    private int currentFocusPoint = -1;
    public CalibrateRobotDialog CalibrateRobotDialog;
    private GameObject model;
    public ButtonWithTooltip CalibrateBtn;

    private bool automaticPointSelection;

    private ActionObject currentObject;

    public ConfirmationDialog ConfirmationDialog;

    public bool AimingInProgress;

    public GameObject Sphere;

    private List<AimingPointSphere> spheres = new();

    private void Update() {
        if (!AimingInProgress || !automaticPointSelection)
            return;
        if (GameManager.Instance.GetGameState() != GameManager.GameStateEnum.SceneEditor) {
            AimingInProgress = false;
            return;
        }
        float maxDist = float.MaxValue;
        int closestPoint = 0;
        foreach (AimingPointSphere sphere in spheres) {
            float dist = Vector3.Distance(sphere.transform.position, SceneManager.Instance.SelectedEndEffector.transform.position);
            if (dist < maxDist) {
                closestPoint = sphere.Index;
                maxDist = dist;
            }
        }

        if (closestPoint != currentFocusPoint) {
            if (currentFocusPoint >= 0 && currentFocusPoint < currentObject.ActionObjectMetadata.ObjectModel.Mesh.FocusPoints.Count)
                spheres[currentFocusPoint].UnHighlight();
            spheres[closestPoint].Highlight();
            currentFocusPoint = closestPoint;
            UpdateCurrentPointLabel();
        }

    }

    private void Start() {
        Debug.Assert(NextButton != null);
        Debug.Assert(PreviousButton != null);
        Debug.Assert(FocusObjectDoneButton != null);
        Debug.Assert(StartObjectFocusingButton != null);
        Debug.Assert(SavePositionButton != null);
        Debug.Assert(CurrentPointLabel != null);
        Debug.Assert(UpdatePositionBlockMesh != null);
        Debug.Assert(UpdatePositionBlockVO != null);
        List<string> pivots = new();
        foreach (string item in Enum.GetNames(typeof(UpdateObjectPoseUsingRobotRequestArgs.PivotEnum))) {
            pivots.Add(item);
        }
        PivotList.PutData(pivots, "Middle", OnPivotChanged);
        AimingInProgress = false;
        CommunicationManager.Instance.Client.ProcessState += CommunicationManager.SafeEventHandler<ProcessStateEventArgs>(OnCameraOrRobotCalibrationEvent);
        SceneManager.Instance.OnSceneStateEvent += OnSceneStateEvent;
    }

    private void OnSceneStateEvent(object sender, SceneStateEventArgs args) {
        _ = Hide();
    }

    public async override Task<bool> Show(InteractiveObject obj, bool lockTree) {
        if (obj is ActionObject actionObject) {
            if (actionObject.IsRobot()) {
                return false;
            } else {
                if (await base.Show(obj, lockTree) && await SceneManager.Instance.SelectedRobot.WriteLock(false))
                    lockedObjects.Add(SceneManager.Instance.SelectedRobot.GetInteractiveObject());
                else
                    return false;
                currentObject = actionObject;
            }
        } else {
            return false;
        }

        await UpdateMenu();
        EditorHelper.EnableCanvasGroup(CanvasGroup, true);
        RobotInfoMenu.Instance.Show();
        return true;
    }

    public override async Task Hide() {
        await base.Hide();
        foreach (AimingPointSphere sphere in spheres) {
            if (sphere != null) {
                Destroy(sphere.gameObject);
            }
        }
        if (!IsVisible)
            return;
        HideModelOnEE();
        EditorHelper.EnableCanvasGroup(CanvasGroup, false);

        spheres.Clear();
        currentObject = null;

        RobotInfoMenu.Instance.Hide();
    }

    private void OnCameraOrRobotCalibrationEvent(object sender, ProcessStateEventArgs args) {
        if (args.Data.State == ProcessStateData.StateEnum.Finished) {
            Notifications.Instance.ShowToastMessage("Calibration finished successfuly");
            GameManager.Instance.HideLoadingScreen();
        } else if (args.Data.State == ProcessStateData.StateEnum.Failed) {
            Notifications.Instance.ShowNotification("Calibration failed", args.Data.Message);
            GameManager.Instance.HideLoadingScreen();
        }
    }

    public async Task UpdateMenu() {
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
            if (SceneManager.Instance.GetCamerasNames().Count > 0) {
                CalibrateBtn.SetInteractivity(true);
            } else {
                CalibrateBtn.SetInteractivity(false, "Could not calibrate robot without camera");
            }
        } else if (currentObject.IsCamera()) {
            CalibrateBtn.gameObject.SetActive(true);
            CalibrateBtn.SetDescription("Calibrate camera");
            CalibrateBtn.Button.onClick.AddListener(() => ShowCalibrateCameraDialog());
        }

        if (SceneManager.Instance.IsRobotAndEESelected()) {

            if (currentObject.ActionObjectMetadata.ObjectModel?.Type == ObjectModel.TypeEnum.Mesh &&
                currentObject.ActionObjectMetadata.ObjectModel.Mesh.FocusPoints?.Count > 0) {
                UpdatePositionBlockVO.SetActive(false);
                UpdatePositionBlockMesh.SetActive(true);
                int idx = 0;
                foreach (AimingPointSphere sphere in spheres) {
                    if (sphere != null) {
                        Destroy(sphere.gameObject);
                    }
                }
                spheres.Clear();
                foreach (Pose point in currentObject.ActionObjectMetadata.ObjectModel.Mesh.FocusPoints) {
                    AimingPointSphere sphere = Instantiate(Sphere, currentObject.transform).GetComponent<AimingPointSphere>();
                    sphere.transform.localScale = new Vector3(0.02f, 0.02f, 0.02f);
                    sphere.transform.localPosition = TransformConvertor.ROSToUnity(DataHelper.PositionToVector3(point.Position));
                    sphere.transform.localRotation = TransformConvertor.ROSToUnity(DataHelper.OrientationToQuaternion(point.Orientation));
                    sphere.Init(idx, $"Aiming point #{idx}");
                    spheres.Add(sphere);
                    ++idx;
                }
                try {
                    List<int> finishedIndexes;
                    var result = await CommunicationManager.Instance.Client.ObjectAimingAddPointAsync(new ObjectAimingAddPointRequestArgs(0), true);
                    finishedIndexes = result.Data.FinishedIndexes;
                    foreach (AimingPointSphere sphere in spheres) {
                        sphere.SetAimed(finishedIndexes.Contains(sphere.Index));
                    }
                    if (!automaticPointSelection)
                        currentFocusPoint = 0;
                    StartObjectFocusingButton.SetInteractivity(false, "Already started");
                    SavePositionButton.SetInteractivity(true);
                    CancelAimingButton.SetInteractivity(true);
                    await CheckDoneBtn();
                    AimingInProgress = true;
                    if (currentObject is ActionObject3D actionObject3D)
                        actionObject3D.UnHighlight();
                    UpdateCurrentPointLabel();
                    if (!automaticPointSelection && currentObject.ActionObjectMetadata.ObjectModel.Mesh.FocusPoints.Count > 1) {
                        NextButton.SetInteractivity(true);
                        PreviousButton.SetInteractivity(true);
                        PreviousPoint();
                    }
                } catch (Arcor2ConnectionException ex) {
                    StartObjectFocusingButton.SetInteractivity(true);
                    FocusObjectDoneButton.SetInteractivity(false, "No aiming in progress");
                    NextButton.SetInteractivity(false, "No aiming in progress");
                    PreviousButton.SetInteractivity(false, "No aiming in progress");
                    SavePositionButton.SetInteractivity(false, "No aiming in progress");
                    CancelAimingButton.SetInteractivity(false, "No aiming in progress");
                    AimingInProgress = false;
                    if (currentObject is ActionObject3D actionObject3D)
                        actionObject3D.Highlight();
                }
            } else if (!currentObject.IsRobot() && !currentObject.IsCamera() && currentObject.ActionObjectMetadata.ObjectModel != null) {
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





    }

    private async Task CheckDoneBtn() {
        try {
            await CommunicationManager.Instance.Client.ObjectAimingDoneAsync(true);
            FocusObjectDoneButton.SetInteractivity(true);
        } catch (Arcor2ConnectionException ex) {
            FocusObjectDoneButton.SetInteractivity(false, ex.Message);
        }
    }

    private void OnEEChanged(string eeId) {
        UpdateModelOnEE();
    }

    private void OnPivotChanged(string pivot) {
        UpdateModelOnEE();
    }


    public async void UpdateObjectPosition() {
        if (!SceneManager.Instance.IsRobotAndEESelected()) {
            NotificationsModernUI.Instance.ShowNotification("Failed to update object position", "No robot or end effector available");
            return;
        }
        UpdateObjectPoseUsingRobotRequestArgs.PivotEnum pivot = (UpdateObjectPoseUsingRobotRequestArgs.PivotEnum) Enum.Parse(typeof(UpdateObjectPoseUsingRobotRequestArgs.PivotEnum), (string) PivotList.GetValue());
        string armId = null;
        if (SceneManager.Instance.SelectedRobot.MultiArm())
            armId = SceneManager.Instance.SelectedArmId;
        try {
            await CommunicationManager.Instance.Client.UpdateObjectPoseUsingRobotAsync(new UpdateObjectPoseUsingRobotRequestArgs(currentObject.Data.Id,
                    new RobotArg(SceneManager.Instance.SelectedRobot.GetId(), SceneManager.Instance.SelectedEndEffector.GetName(), armId), pivot));
        } catch (Arcor2ConnectionException ex) {
            Notifications.Instance.ShowNotification("Failed to update object position", ex.Message);
        }

    }

    public async void CancelAiming() {
        try {
            await CommunicationManager.Instance.Client.ObjectAimingCancelAsync();
            AimingInProgress = false;
            if (currentObject is ActionObject3D actionObject3D)
                actionObject3D.Highlight();
            if (currentFocusPoint >= 0 && currentFocusPoint < spheres.Count)
                spheres[currentFocusPoint].UnHighlight();
            UpdateCurrentPointLabel();
            await UpdateMenu();
        } catch (Arcor2ConnectionException ex) {
            Notifications.Instance.ShowNotification("Failed to cancel aiming", ex.Message);
        }
    }

    public void SetAutomaticPointSelection(bool automatic) {
        automaticPointSelection = automatic;
        if (automatic) {
            NextButton.SetInteractivity(false, "Not available when automatic point selection is active");
            PreviousButton.SetInteractivity(false, "Not available when automatic point selection is active");
        } else {
            if (!AimingInProgress)
                return;

            NextButton.SetInteractivity(currentFocusPoint < currentObject.ActionObjectMetadata.ObjectModel.Mesh.FocusPoints.Count - 1, "Selected point is the first one");
            PreviousButton.SetInteractivity(currentFocusPoint > 0, "Selected point is the first one");
        }
    }


    public async void StartObjectFocusing() {
        if (!SceneManager.Instance.IsRobotAndEESelected()) {
            NotificationsModernUI.Instance.ShowNotification("Failed to update object position", "No robot or end effector available");
            return;
        }
        try {
            AimingInProgress = true;
            string armId = null;
            if (SceneManager.Instance.SelectedRobot.MultiArm())
                armId = SceneManager.Instance.SelectedArmId;
            await CommunicationManager.Instance.Client.ObjectAimingStartAsync(new ObjectAimingStartRequestArgs(currentObject.Data.Id,
                new RobotArg(
                    SceneManager.Instance.SelectedRobot.GetId(),
                    SceneManager.Instance.SelectedEndEffector.GetName(),
                    armId)
                )
            );
            currentFocusPoint = 0;
            UpdateCurrentPointLabel();
            //TODO: ZAJISTIT ABY MENU NEŠLO ZAVŘÍT když běží focusing - ideálně nějaký dialog

            await CheckDoneBtn();
            SavePositionButton.SetInteractivity(true);
            CancelAimingButton.SetInteractivity(true);
            StartObjectFocusingButton.SetInteractivity(false, "Already aiming");
            if (currentObject is ActionObject3D actionObject3D)
                actionObject3D.UnHighlight();
            if (!automaticPointSelection && currentObject.ActionObjectMetadata.ObjectModel.Mesh.FocusPoints.Count > 1) {
                NextButton.SetInteractivity(true);
                PreviousButton.SetInteractivity(true);
                PreviousPoint();
            }
            foreach (AimingPointSphere sphere in spheres) {
                sphere.SetAimed(false);
            }
        } catch (Arcor2ConnectionException ex) {
            NotificationsModernUI.Instance.ShowNotification("Failed to start object focusing", ex.Message);
            CurrentPointLabel.text = "";
            currentFocusPoint = -1;
            AimingInProgress = false;
            if (currentObject is ActionObject3D actionObject3D)
                actionObject3D.Highlight();
            if (ex.Message == "Focusing already started.") { //TODO HACK! find better solution
                FocusObjectDone();
            }
        }
    }

    public async void SavePosition() {
        if (currentFocusPoint < 0)
            return;
        try {
            await CommunicationManager.Instance.Client.ObjectAimingAddPointAsync(new ObjectAimingAddPointRequestArgs(currentFocusPoint));
            spheres[currentFocusPoint].SetAimed(true);
            await CheckDoneBtn();
        } catch (Arcor2ConnectionException ex) {
            NotificationsModernUI.Instance.ShowNotification("Failed to save current position", ex.Message);
        }


    }

    public async void FocusObjectDone() {
        try {
            CurrentPointLabel.text = "";

            // TODO: znovupovolit zavření menu
            currentFocusPoint = -1;

            await CommunicationManager.Instance.Client.ObjectAimingDoneAsync();
            FocusObjectDoneButton.SetInteractivity(false, "No aiming in progress");
            CancelAimingButton.SetInteractivity(false, "No aiming in progress");
            NextButton.SetInteractivity(false, "No aiming in progress");
            PreviousButton.SetInteractivity(false, "No aiming in progress");
            SavePositionButton.SetInteractivity(false, "No aiming in progress");
            StartObjectFocusingButton.SetInteractivity(true);
            if (currentObject is ActionObject3D actionObject3D)
                actionObject3D.Highlight();
            AimingInProgress = false;
        } catch (Arcor2ConnectionException ex) {
            NotificationsModernUI.Instance.ShowNotification("Failed to focus object", ex.Message);
        }
    }

    public void NextPoint() {
        if (currentFocusPoint >= 0 && currentFocusPoint <= spheres.Count)
            spheres[currentFocusPoint].UnHighlight();
        spheres[currentFocusPoint].UnHighlight();
        currentFocusPoint = Math.Min(currentFocusPoint + 1, currentObject.ActionObjectMetadata.ObjectModel.Mesh.FocusPoints.Count - 1);
        PreviousButton.SetInteractivity(true);
        if (currentFocusPoint == currentObject.ActionObjectMetadata.ObjectModel.Mesh.FocusPoints.Count - 1) {
            NextButton.SetInteractivity(false, "Selected point is the last one");
        } else {
            NextButton.SetInteractivity(true);
        }
        UpdateCurrentPointLabel();
        spheres[currentFocusPoint].Highlight();
    }

    public void PreviousPoint() {
        if (currentFocusPoint >= 0 && currentFocusPoint <= spheres.Count)
            spheres[currentFocusPoint].UnHighlight();
        currentFocusPoint = Math.Max(currentFocusPoint - 1, 0);
        NextButton.SetInteractivity(true);
        if (currentFocusPoint == 0) {
            PreviousButton.SetInteractivity(false, "Selected point is the first one");
        } else {
            PreviousButton.SetInteractivity(true);
        }
        UpdateCurrentPointLabel();
        spheres[currentFocusPoint].Highlight();
    }

    private void UpdateCurrentPointLabel() {
        if (!AimingInProgress)
            CurrentPointLabel.text = "";
        else
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
            throw new Arcor2ConnectionException("Robot or end effector not selected!");
        }

        try {
            model.transform.parent = SceneManager.Instance.SelectedEndEffector.gameObject.transform;

            switch ((UpdateObjectPoseUsingRobotRequestArgs.PivotEnum) Enum.Parse(typeof(UpdateObjectPoseUsingRobotRequestArgs.PivotEnum), (string) PivotList.GetValue())) {
                case UpdateObjectPoseUsingRobotRequestArgs.PivotEnum.Top:
                    model.transform.localPosition = new Vector3(0, model.transform.localScale.y / 2, 0);
                    break;
                case UpdateObjectPoseUsingRobotRequestArgs.PivotEnum.Bottom:
                    model.transform.localPosition = new Vector3(0, -model.transform.localScale.y / 2, 0);
                    break;
                case UpdateObjectPoseUsingRobotRequestArgs.PivotEnum.Middle:
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
        if (CalibrateRobotDialog.Init(SceneManager.Instance.GetCamerasNames(), currentObject.Data.Id))
            CalibrateRobotDialog.Open();
    }


    public void ShowCalibrateCameraDialog() {
        ConfirmationDialog.Open("Camera calibration", "Are you sure you want to initiate camera calibration?",
            async () => await CalibrateCamera(), () => ConfirmationDialog.Close());
    }

    public async Task CalibrateCamera() {
        try {
            ConfirmationDialog.Close();
            GameManager.Instance.ShowLoadingScreen("Calibrating camera...");
            var response = await CommunicationManager.Instance.Client.CalibrateCameraAsync(new CalibrateCameraRequestArgs(currentObject.Data.Id));
            if (!response.Result) {
                GameManager.Instance.HideLoadingScreen();
                Notifications.Instance.ShowNotification("Failed to calibrate camera", string.Join(',', response.Messages));
                ConfirmationDialog.Close();
            }
        } catch (Arcor2ConnectionException ex) {
            GameManager.Instance.HideLoadingScreen();
            Notifications.Instance.ShowNotification("Failed to calibrate camera", ex.Message);
            ConfirmationDialog.Close();
        }
    }

    public void OpenSteppingMenu() {
        RobotSteppingMenu.Instance.Show(true, "Go back to aiming menu", () => EditorHelper.EnableCanvasGroup(CanvasGroup, true));
        EditorHelper.EnableCanvasGroup(CanvasGroup, false);
    }

    public void Highlight(bool enable) {
        if (currentObject != null && currentObject is ActionObject3D actionObject) {
            if (enable)
                actionObject.Highlight();
            else
                actionObject.UnHighlight();
        }
    }

}
