using System.Threading.Tasks;
using Base;
using Newtonsoft.Json;
using UnityEngine;

public class LeftMenuPackage : LeftMenu {

    public ButtonWithTooltip PauseBtn, ResumeBtn, StepBtn;

    protected override void Awake() {
        base.Awake();
        Base.GameManager.Instance.OnRunPackage += OnOpenProjectRunning;
        Base.GameManager.Instance.OnPausePackage += OnPausePackage;
        Base.GameManager.Instance.OnResumePackage += OnResumePackage;
        Base.GameManager.Instance.OnStopPackage += OnStopPackage;
        SceneManager.Instance.OnSceneStateEvent += OnSceneStateEvent;
    }

    private void OnStopPackage(object sender, System.EventArgs e) {
        UpdateVisibility(GameManager.GameStateEnum.ProjectEditor);
    }

    private void OnResumePackage(object sender, ProjectMetaEventArgs args) {
        ResumeBtn.gameObject.SetActive(false);
        PauseBtn.gameObject.SetActive(true);
        PauseBtn.SetInteractivity(true);
    }

    private void OnPausePackage(object sender, ProjectMetaEventArgs args) {
        ResumeBtn.gameObject.SetActive(true);
        PauseBtn.gameObject.SetActive(false);
        ResumeBtn.SetInteractivity(true);
    }

    private void OnOpenProjectRunning(object sender, ProjectMetaEventArgs args) {
        ResumeBtn.gameObject.SetActive(false);
        PauseBtn.gameObject.SetActive(true);
        CloseButton.SetInteractivity(true);
        PauseBtn.SetInteractivity(true);
        EditorInfo.text = "Package: " + args.Name;
        UpdateVisibility();
    }

    protected override void OnSceneStateEvent(object sender, SceneStateEventArgs args) {
        UpdateVisibility();
    }

    public override void UpdateBuildAndSaveBtns() {
        // nothing to do here when package is running
    }

    public override void UpdateVisibility() {
        UpdateVisibility(GameManager.Instance.GetGameState());
    }

    public void UpdateVisibility(GameManager.GameStateEnum newGameState) {
        
        if (newGameState == GameManager.GameStateEnum.PackageRunning &&
            MainMenu.Instance.CurrentState() == DanielLochner.Assets.SimpleSideMenu.SimpleSideMenu.State.Closed) {
            UpdateVisibility(true);            
        } else {
            UpdateVisibility(false);
        }
    }

    public override void UpdateVisibility(bool visible, bool force = false) {
        base.UpdateVisibility(visible, force);
        if (GameManager.Instance.GetGameState() == GameManager.GameStateEnum.PackageRunning)
            AREditorResources.Instance.StartStopSceneBtn.gameObject.SetActive(false);
    }

    public void ShowStopPackageDialog() {
        GameManager.Instance.HideLoadingScreen();
        ConfirmationDialog.Open("Stop package",
                        "Are you sure you want to stop execution of current package?",
                        () => StopPackage(),
                        () => ConfirmationDialog.Close());
        
    }

    public void StopPackage() {
        CloseButton.SetInteractivity(false, "Stopping package");
        GameManager.Instance.ShowLoadingScreen();
        WebsocketManager.Instance.StopPackage(StopPackageCallback);
        
    }


    private void StopPackageCallback(string _, string data) {
        IO.Swagger.Model.StopPackageResponse response = JsonConvert.DeserializeObject<IO.Swagger.Model.StopPackageResponse>(data);
        CloseButton.SetInteractivity(true);
        if (!response.Result) {
            Notifications.Instance.ShowNotification("Failed to stop package.", response.Messages.Count > 0 ? response.Messages[0] : "Unknown error");
            GameManager.Instance.HideLoadingScreen();

        }
    } 

    public async void PausePackage() {
        PauseBtn.SetInteractivity(false, "Pausing package");
        if (await GameManager.Instance.PausePackage()) {
            PauseBtn.SetInteractivity(true);
        }
    }

    public async void ResumePackage() {
        ResumeBtn.SetInteractivity(false, "Resuming package");
        if (await GameManager.Instance.ResumePackage()) {
            ResumeBtn.SetInteractivity(true);
        }
    }

    protected async override Task UpdateBtns(InteractiveObject obj) {
        try {
            if (CanvasGroup.alpha == 0) {
                return;
            }

            if (requestingObject || obj == null) {
                SelectedObjectText.text = "";
                OpenMenuButton.SetInteractivity(false, "No object selected");
                CalibrationButton.SetInteractivity(false, "No object selected");
            } else if (obj.IsLocked) {
                SelectedObjectText.text = obj.GetName() + "\n" + obj.GetObjectTypeName();
                OpenMenuButton.SetInteractivity(false, "Object is locked");
                CalibrationButton.SetInteractivity(false, "Object is locked");
            } else {
                SelectedObjectText.text = obj.GetName() + "\n" + obj.GetObjectTypeName();
                CalibrationButton.SetInteractivity(obj.GetType() == typeof(Recalibrate) ||
                    obj.GetType() == typeof(CreateAnchor) || obj.GetType() == typeof(RecalibrateUsingServer), "Selected object is not calibration cube");
                if (obj is Action3D action) {
                    OpenMenuButton.SetInteractivity(action.Parameters.Count > 0, "Action has no parameters");
                } else {
                    OpenMenuButton.SetInteractivity(obj.HasMenu(), "Selected object has no menu");
                }
            }
        } finally {
            previousUpdateDone = true;
        }
    }

    public override void CopyObjectClick() {
        throw new System.NotImplementedException();
    }

    public async void StepAction() {
        try {
            await WebsocketManager.Instance.StepAction();
        } catch (RequestFailedException ex) {
            Notifications.Instance.ShowNotification("Failed to step", ex.Message);
        }
    } 
}
