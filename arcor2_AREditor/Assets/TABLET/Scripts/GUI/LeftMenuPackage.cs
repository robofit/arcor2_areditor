using System.Threading.Tasks;
using Base;
using UnityEngine;

public class LeftMenuPackage : LeftMenu {

    public ButtonWithTooltip PauseBtn, ResumeBtn;

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
        Debug.LogError("Pause");
        ResumeBtn.gameObject.SetActive(true);
        PauseBtn.gameObject.SetActive(false);
        ResumeBtn.SetInteractivity(true);
    }

    private void OnOpenProjectRunning(object sender, ProjectMetaEventArgs args) {
        Debug.LogError("open package running");
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
        Debug.LogError(GameManager.Instance.GetGameState());
        UpdateVisibility(GameManager.Instance.GetGameState());
    }

    public void UpdateVisibility(GameManager.GameStateEnum newGameState) {
        
        if (newGameState == GameManager.GameStateEnum.PackageRunning &&
            MenuManager.Instance.MainMenu.CurrentState == DanielLochner.Assets.SimpleSideMenu.SimpleSideMenu.State.Closed) {
            UpdateVisibility(true);
        } else {
            UpdateVisibility(false);
        }
    }

    public void ShowStopPackageDialog() {
        GameManager.Instance.HideLoadingScreen();
        ConfirmationDialog.Open("Stop package",
                        "Are you sure you want to stop execution of current package?",
                        () => StopPackage(),
                        () => ConfirmationDialog.Close());
        
    }

    public async void StopPackage() {
        CloseButton.SetInteractivity(false, "Stopping package");
        if (await GameManager.Instance.StopPackage()) {
            CloseButton.SetInteractivity(true);
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

            if (requestingObject || obj == null) {
                SelectedObjectText.text = "";
                OpenMenuButton.SetInteractivity(false, "No object selected");
            } else if (obj.IsLocked) {
                SelectedObjectText.text = obj.GetName() + "\n" + obj.GetObjectTypeName();
                OpenMenuButton.SetInteractivity(false, "Object is locked");
            } else {
                SelectedObjectText.text = obj.GetName() + "\n" + obj.GetObjectTypeName();
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
}
