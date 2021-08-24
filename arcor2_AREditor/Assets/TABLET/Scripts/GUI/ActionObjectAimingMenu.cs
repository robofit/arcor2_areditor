using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Base;
using IO.Swagger.Model;
using UnityEngine;
using UnityEngine.UI;
using static IO.Swagger.Model.UpdateObjectPoseUsingRobotRequestArgs;

public class ActionObjectAimingMenu : Base.Singleton<ActionObjectAimingMenu>
{
    public DropdownParameter PivotList;
    public ButtonWithTooltip NextButton, PreviousButton, FocusObjectDoneButton, StartObjectFocusingButton, SavePositionButton, CancelAimingButton;
    public TMPro.TMP_Text CurrentPointLabel;
    public GameObject UpdatePositionBlockMesh, UpdatePositionBlockVO;
    public SwitchComponent ShowModelSwitch;
    private int currentFocusPoint = -1;
    public CalibrateRobotDialog CalibrateRobotDialog;
    private GameObject model;
    public ButtonWithTooltip CalibrateBtn;

    private bool automaticPointSelection;

    private ActionObject currentObject;

    public ConfirmationDialog ConfirmationDialog;

    public CanvasGroup CanvasGroup;

    public bool AimingInProgress;

    public GameObject Sphere;

    private List<AimingPointSphere> spheres = new List<AimingPointSphere>();

    private void Update() {
        if (!AimingInProgress || !automaticPointSelection)
            return;
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
        List<string> pivots = new List<string>();
        foreach (string item in Enum.GetNames(typeof(PivotEnum))) {
            pivots.Add(item);
        }
        PivotList.PutData(pivots, "Middle", OnPivotChanged);
        AimingInProgress = false;
        WebsocketManager.Instance.OnProcessStateEvent += OnCameraOrRobotCalibrationEvent;
        SceneManager.Instance.OnSceneStateEvent += OnSceneStateEvent;
    }

    private void OnSceneStateEvent(object sender, SceneStateEventArgs args) {
        Hide(false);
        
    }

    public async void Show(ActionObject actionObject) {
        if (actionObject.IsRobot()) {
            if (!await actionObject.WriteLock(false))
                return;
        } else {
            if (!await actionObject.WriteLock(false) || !await SceneManager.Instance.SelectedRobot.WriteLock(false))
                return;
        }
        currentObject = actionObject;
        await UpdateMenu();
        EditorHelper.EnableCanvasGroup(CanvasGroup, true);
        RobotInfoMenu.Instance.Show();
    }

    public async void Hide(bool unlock = true) {
        if (CanvasGroup.alpha == 0)
            return;
        HideModelOnEE();
        EditorHelper.EnableCanvasGroup(CanvasGroup, false);
        foreach (AimingPointSphere sphere in spheres) {
            if (sphere != null) {
                Destroy(sphere.gameObject);
            }
        }
        spheres.Clear();
        if (currentObject != null) {
            if (unlock) {
                await currentObject.WriteUnlock();
                if (!currentObject.IsRobot()) {
                    await SceneManager.Instance.SelectedRobot.WriteUnlock();
                }
            }
            currentObject = null;
        }
        RobotInfoMenu.Instance.Hide();
    }

    private void OnCameraOrRobotCalibrationEvent(object sender, ProcessStateEventArgs args) {
        if (args.Data.State == IO.Swagger.Model.ProcessStateData.StateEnum.Finished) {
            Notifications.Instance.ShowToastMessage("Calibration finished successfuly");
            GameManager.Instance.HideLoadingScreen();
        } else if (args.Data.State == IO.Swagger.Model.ProcessStateData.StateEnum.Failed) {
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

            if (currentObject.ActionObjectMetadata.ObjectModel?.Type == IO.Swagger.Model.ObjectModel.TypeEnum.Mesh &&
                currentObject.ActionObjectMetadata.ObjectModel.Mesh.FocusPoints?.Count > 0) {
                UpdatePositionBlockVO.SetActive(false);
                UpdatePositionBlockMesh.SetActive(true);
                int idx = 0;
                
                foreach (IO.Swagger.Model.Pose point in currentObject.ActionObjectMetadata.ObjectModel.Mesh.FocusPoints) {
                    AimingPointSphere sphere = Instantiate(Sphere, currentObject.transform).GetComponent<AimingPointSphere>();
                    sphere.transform.localScale = new Vector3(0.02f, 0.02f, 0.02f);
                    sphere.transform.localPosition = DataHelper.PositionToVector3(point.Position);
                    sphere.transform.localRotation = DataHelper.OrientationToQuaternion(point.Orientation);
                    sphere.Init(idx, $"Aiming point #{idx}");
                    spheres.Add(sphere);
                    ++idx;
                }
                try {
                    List<int> finishedIndexes = await WebsocketManager.Instance.ObjectAimingAddPoint(0, true);
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
                    UpdateCurrentPointLabel();
                    if (!automaticPointSelection && currentObject.ActionObjectMetadata.ObjectModel.Mesh.FocusPoints.Count > 1) {
                        NextButton.SetInteractivity(true);
                        PreviousButton.SetInteractivity(true);
                        PreviousPoint();
                    }
                } catch (RequestFailedException ex) {
                    StartObjectFocusingButton.SetInteractivity(true);
                    FocusObjectDoneButton.SetInteractivity(false, "No aiming in progress");
                    NextButton.SetInteractivity(false, "No aiming in progress");
                    PreviousButton.SetInteractivity(false, "No aiming in progress");
                    SavePositionButton.SetInteractivity(false, "No aiming in progress");
                    CancelAimingButton.SetInteractivity(false, "No aiming in progress");
                    AimingInProgress = false;
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
            await WebsocketManager.Instance.ObjectAimingDone(true);
            FocusObjectDoneButton.SetInteractivity(true);
        } catch (RequestFailedException ex) {
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

    public async void CancelAiming() {
        try {
            await WebsocketManager.Instance.CancelObjectAiming();
            AimingInProgress = false;
            if (currentFocusPoint >= 0 && currentFocusPoint < spheres.Count)
                spheres[currentFocusPoint].UnHighlight();
            UpdateCurrentPointLabel();
            await UpdateMenu();
        } catch (RequestFailedException ex) {
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
            Base.NotificationsModernUI.Instance.ShowNotification("Failed to update object position", "No robot or end effector available");
            return;
        }
        if (! await currentObject.WriteLock(true) || ! await SceneManager.Instance.SelectedRobot.WriteLock(false))
            Notifications.Instance.ShowNotification("Failed to start aiming", "Object or robot could not be locked");
        try {
            AimingInProgress = true;
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

            await CheckDoneBtn();
            SavePositionButton.SetInteractivity(true);
            CancelAimingButton.SetInteractivity(true);
            StartObjectFocusingButton.SetInteractivity(false, "Already aiming");
            if (!automaticPointSelection && currentObject.ActionObjectMetadata.ObjectModel.Mesh.FocusPoints.Count > 1) {
                NextButton.SetInteractivity(true);
                PreviousButton.SetInteractivity(true);
                PreviousPoint();
            }
            foreach (AimingPointSphere sphere in spheres) {
                sphere.SetAimed(false);
            }
        } catch (Base.RequestFailedException ex) {
            Base.NotificationsModernUI.Instance.ShowNotification("Failed to start object focusing", ex.Message);
            CurrentPointLabel.text = "";
            currentFocusPoint = -1;
            AimingInProgress = false;
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
            spheres[currentFocusPoint].SetAimed(true);
            await CheckDoneBtn();
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
            
            await WebsocketManager.Instance.ObjectAimingDone();
            FocusObjectDoneButton.SetInteractivity(false, "No aiming in progress");
            NextButton.SetInteractivity(false, "No aiming in progress");
            PreviousButton.SetInteractivity(false, "No aiming in progress");
            SavePositionButton.SetInteractivity(false, "No aiming in progress");
            StartObjectFocusingButton.SetInteractivity(true);
            AimingInProgress = false;
        } catch (Base.RequestFailedException ex) {
            Base.NotificationsModernUI.Instance.ShowNotification("Failed to focus object", ex.Message);
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
            await WebsocketManager.Instance.CalibrateCamera(currentObject.Data.Id);
        } catch (RequestFailedException ex) {
            GameManager.Instance.HideLoadingScreen();
            Notifications.Instance.ShowNotification("Failed to calibrate camera", ex.Message);
            ConfirmationDialog.Close();
        }
    }

    public void OpenSteppingMenu() {
        RobotSteppingMenu.Instance.Show(true, "Go back to aiming menu", () => EditorHelper.EnableCanvasGroup(CanvasGroup, true));
        EditorHelper.EnableCanvasGroup(CanvasGroup, false);
    }

}
