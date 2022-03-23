using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Base;
using UnityEngine;
using UnityEngine.UI;
using IO.Swagger.Model;
using Newtonsoft.Json;

public class MainSettingsMenu : Singleton<MainSettingsMenu>
{
    public GameObject ContainerEditor, ContainerConstants, ContentEditor, ContentConstants, ContainerAR, ContentAR, ConstantButtonPrefab;
    public ButtonWithTooltip SwitchToProjectParametersBtn;
    public Image SwitchToProjectParametersBtnImage;

    public List<GameObject> ProjectRelatedSettings = new List<GameObject>();

    public CanvasGroup CanvasGroup;

    public SwitchComponent Interactibility, APOrientationsVisibility, RobotsEEVisible, ConnectionsSwitch;
    public SwitchComponent AutoCalibration, Trackables, VRMode, CalibrationElements;
    [SerializeField]
    private Slider APSizeSlider, ActionObjectsVisibilitySlider;
    [SerializeField]
    private LabeledInput recalibrationTime;

    public ManualTooltip CalibrationElementsTooltip;
    public ManualTooltip AutoCalibTooltip;

    //for constants
    private EditProjectParameterDialog EditConstantDialog;
    //private Action3D currentAction;

    public LinkableInput ProjectServiceURI;
    public ButtonWithTooltip ResetProjectServiceURIButton;
    private void Start() {
        SceneManager.Instance.OnLoadScene += OnLoadScene;
        ProjectManager.Instance.OnLoadProject += OnLoadProject;
        EditConstantDialog = (EditProjectParameterDialog) AREditorResources.Instance.EditProjectParameterDialog;
        ConnectionsSwitch.AddOnValueChangedListener((_) => AREditorResources.Instance.LeftMenuProject.UpdateBtns());
        ConnectionsSwitch.AddOnValueChangedListener(ProjectManager.Instance.SetActionInputOutputVisibility);
        WebsocketManager.Instance.OnProjectParameterAdded += OnProjectParameterAdded;
        WebsocketManager.Instance.OnProjectParameterRemoved += OnProjectParameterRemoved;

    }

    public string GetProjectServiceURI(bool complete = true) {
        string uri = PlayerPrefsHelper.LoadString("ProjectServiceURI", "");
        string suffix = complete ? "/files/" : "";

        // TODO this could (should?) work without connection to the server
        Debug.Assert(!string.IsNullOrEmpty(WebsocketManager.Instance.GetServerDomain()), "GetProjectServiceURI was probably used without connection to the server.");

        if (string.IsNullOrEmpty(uri))
            return "http://" + WebsocketManager.Instance.GetServerDomain() + ":6790" + suffix;
        else
            return uri + suffix;
    }
    private void OnLoadProject(object sender, EventArgs e) {
        OnProjectOrSceneLoaded(true);
    }

    private void OnLoadScene(object sender, EventArgs e) {
        OnProjectOrSceneLoaded(false);
    }

#if UNITY_ANDROID && AR_ON
    private void OnEnable() {
        CalibrationManager.Instance.OnARCalibrated += OnARCalibrated;
        CalibrationManager.Instance.OnARRecalibrate += OnARRecalibrate;
    }

    private void OnDisable() {
        CalibrationManager.Instance.OnARCalibrated -= OnARCalibrated;
        CalibrationManager.Instance.OnARRecalibrate -= OnARRecalibrate;
    }
#endif

   

    private void OnProjectOrSceneLoaded(bool project) {
        
        foreach (GameObject obj in ProjectRelatedSettings) {
            obj.SetActive(project);
        }
        if (project) {
            APSizeSlider.gameObject.SetActive(true);
            APOrientationsVisibility.gameObject.SetActive(true);
            APSizeSlider.value = ProjectManager.Instance.APSize;
            APOrientationsVisibility.SetValue(Base.ProjectManager.Instance.APOrientationsVisible);

            SwitchToProjectParametersBtn.SetInteractivity(true);
            SwitchToProjectParametersBtnImage.color = Color.white;
            GenerateParameterButtons();
            WebsocketManager.Instance.OnProjectParameterAdded += OnProjectParameterAdded;
            WebsocketManager.Instance.OnProjectParameterRemoved += OnProjectParameterRemoved;
        } else {
            APSizeSlider.gameObject.SetActive(false);
            APOrientationsVisibility.gameObject.SetActive(false);
            SwitchToProjectParametersBtn.SetInteractivity(false, "Project parameters are available only in project editor.");
            SwitchToProjectParametersBtnImage.color = Color.gray;
        }

        Interactibility.SetValue(Base.SceneManager.Instance.ActionObjectsInteractive);
        RobotsEEVisible.SetValue(Base.SceneManager.Instance.RobotsEEVisible, false);
        ActionObjectsVisibilitySlider.SetValueWithoutNotify(SceneManager.Instance.ActionObjectsVisibility * 100f);

#if UNITY_ANDROID && AR_ON
        recalibrationTime.SetValue(CalibrationManager.Instance.AutoRecalibrateTime);
        Trackables.SetValue(PlayerPrefsHelper.LoadBool("control_box_display_trackables", false));
        CalibrationElements.Interactable = false;
        CalibrationElements.SetValue(true);
        CalibrationElementsTooltip.DisplayAlternativeDescription = true;


        bool useAutoCalib = PlayerPrefsHelper.LoadBool("control_box_autoCalib", true);

        AutoCalibTooltip.DisplayAlternativeDescription = useAutoCalib;


        AutoCalibration.SetValue(useAutoCalib);
        // If the toggle is unchanged, we need to manually call the EnableAutoReCalibration function.
        // If the toggle has changed, the function will be called automatically. So we need to avoid calling it twice.
        if (((bool) AutoCalibration.GetValue() && useAutoCalib) || (!(bool) AutoCalibration.GetValue() && !useAutoCalib)) {
            EnableAutoReCalibration(useAutoCalib);
        } 

#endif
        ConnectionsSwitch.SetValue(PlayerPrefsHelper.LoadBool("control_box_display_connections", true));
        recalibrationTime.SetValue(PlayerPrefsHelper.LoadString("/autoCalib/recalibrationTime", "120"));
        string uri = PlayerPrefsHelper.LoadString("ProjectServiceURI", "");
        ProjectServiceURI.Input.SetValue(GetProjectServiceURI(false));
        if (string.IsNullOrEmpty(uri)) {
            ResetProjectServiceURIButton.SetInteractivity(false, "Default value is already set");
        } else {
            ResetProjectServiceURIButton.SetInteractivity(true);
        }
            

    }

    public void SwitchToEditor() {
        ContainerConstants.SetActive(false);
        ContainerAR.SetActive(false);
        ContainerEditor.SetActive(true);
    }

    public void SwitchToConstants() {
        ContainerEditor.SetActive(false);
        ContainerAR.SetActive(false);
        ContainerConstants.SetActive(true);
    }

    public void SwitchToAR() {
        ContainerConstants.SetActive(false);
        ContainerEditor.SetActive(false);
        ContainerAR.SetActive(true);
    }

    public void Show() {

        if (GameManager.Instance.GetGameState() != GameManager.GameStateEnum.ProjectEditor) {
            if (ContainerConstants.activeSelf) //project parameters submenu cannot be opened when project is not opened
                SwitchToEditor();
        } else {
            DestroyConstantButtons();
            GenerateParameterButtons();
        }

        EditorHelper.EnableCanvasGroup(CanvasGroup, true);
    }

    public void Hide() {
        EditorHelper.EnableCanvasGroup(CanvasGroup, false);

        DestroyConstantButtons();
        WebsocketManager.Instance.OnProjectParameterAdded -= OnProjectParameterAdded;
        WebsocketManager.Instance.OnProjectParameterRemoved -= OnProjectParameterRemoved;
    }

    public void SetVisibilityActionObjects() {
        SceneManager.Instance.SetVisibilityActionObjects(ActionObjectsVisibilitySlider.value / 100f);
    }

    public float GetVisibilityActionObjects() {
        return ActionObjectsVisibilitySlider.value / 100f;
    }

    public void ShowAPOrientations() {
        Base.ProjectManager.Instance.ShowAPOrientations();
    }

    public void HideAPOrientations() {
        Base.ProjectManager.Instance.HideAPOrientations();
    }

    public void InteractivityOn() {
        Base.SceneManager.Instance.SetActionObjectsInteractivity(true);
    }

    public void InteractivityOff() {
        Base.SceneManager.Instance.SetActionObjectsInteractivity(false);
    }

    public void ShowRobotsEE() {
        if (!SceneManager.Instance.ShowRobotsEE()) {
            RobotsEEVisible.SetValue(false);
        }
    }

    public void HideRobotsEE() {
        SceneManager.Instance.HideRobotsEE();
    }

    public void SwitchOnExpertMode() {
        GameManager.Instance.ExpertMode = true;
    }

    public void SwitchOffExpertMode() {
        GameManager.Instance.ExpertMode = false;
    }

    public void OnAPSizeChange(float value) {
        ProjectManager.Instance.SetAPSize(value);
    }

    public void OnAutoCalibTimeChange(string value) {
        PlayerPrefsHelper.SaveString("/autoCalib/recalibrationTime", value);
        CalibrationManager.Instance.UpdateAutoCalibTime(float.Parse(value));
    }

    public void EnableAutoReCalibration(bool active) {
#if UNITY_ANDROID && AR_ON
        AutoCalibTooltip.DisplayAlternativeDescription = active;
        CalibrationManager.Instance.EnableAutoReCalibration(active);
#endif
    }


    public void DisplayConnections(bool active) {
        ConnectionManagerArcoro.Instance.DisplayConnections(active);
    }

    public void ToggleVRMode(bool active) {
        if (active) {
            VRModeManager.Instance.EnableVRMode();
        } else {
            VRModeManager.Instance.DisableVRMode();
        }
    }


    public void DisplayTrackables(bool active) {
#if UNITY_ANDROID && AR_ON
        TrackingManager.Instance.DisplayPlanesAndPointClouds(active);
#endif
    }

    public void DisplayCalibrationElements(bool active) {
#if UNITY_ANDROID && AR_ON
        CalibrationManager.Instance.ActivateCalibrationElements(active);
#endif
    }


    /// <summary>
    /// Triggered when the system calibrates = anchor is created (either when user clicks on calibration cube or when system loads the cloud anchor).
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="args"></param>
    private void OnARCalibrated(object sender, CalibrationEventArgs args) {
#if UNITY_ANDROID && AR_ON
        // Activate toggle to enable hiding/displaying calibration cube
        CalibrationElements.Interactable = args.Calibrated;
        CalibrationElementsTooltip.DisplayAlternativeDescription = !args.Calibrated;
#endif
    }


    private void OnARRecalibrate(object sender, EventArgs args) {
#if UNITY_ANDROID && AR_ON
        // Disactivate toggle to disable hiding/displaying calibration cube
        CalibrationElements.Interactable = false;
        CalibrationElementsTooltip.DisplayAlternativeDescription = true;
#endif
    }


    /// <summary>
    /// Called when the user tries to click on the show/hide toggle before the system is calibrated.
    /// </summary>
    public void OnCalibrationElementsToggleClick() {
        if (!CalibrationManager.Instance.Calibrated) {
            if (CalibrationManager.Instance.UsingServerCalibration) {
                Notifications.Instance.ShowNotification("System is not calibrated", "Please locate the visual marker and wait for the calibration to complete automatically.");
            } else {
                Notifications.Instance.ShowNotification("System is not calibrated", "Please locate the visual marker, wait for the calibration cube to show up and click on it, in order to calibrate the system.");
            }
        }
    }

    private void OnDestroy() {
#if UNITY_ANDROID && AR_ON
        PlayerPrefsHelper.SaveBool("control_box_display_trackables", (bool) Trackables.GetValue());
        PlayerPrefsHelper.SaveBool("control_box_autoCalib", (bool) AutoCalibration.GetValue());
#endif
        PlayerPrefsHelper.SaveBool("control_box_display_connections", (bool) ConnectionsSwitch.GetValue());
    }

    public void SetProjectServiceURI(string uri) {
        PlayerPrefsHelper.SaveString("ProjectServiceURI", uri.Trim());
        ResetProjectServiceURIButton.SetInteractivity(true);
    }

    public void ResetProjectServiceURI() {
        PlayerPrefsHelper.SaveString("ProjectServiceURI", "");
        ProjectServiceURI.Input.SetValue(GetProjectServiceURI(false));
        ResetProjectServiceURIButton.SetInteractivity(false, "Default value is already set");
    }




    #region Project parameters

    private void OnProjectParameterRemoved(object sender, ProjectParameterEventArgs args) {
        ProjectParameterButton[] btns = ContentConstants.GetComponentsInChildren<ProjectParameterButton>();
        if (btns != null) {
            foreach (ProjectParameterButton btn in btns.Where(o => o.Id == args.ProjectParameter.Id)) {
                Destroy(btn.gameObject);
                return;
            }
        }
    }

    private void OnProjectParameterAdded(object sender, ProjectParameterEventArgs args) {
        //it needs to be sorted alphabetically, so cannot just add new button
        DestroyConstantButtons();
        GenerateParameterButtons();
    }

    private void GenerateParameterButtons() {
        foreach (ProjectParameter projectParameter in ProjectManager.Instance.ProjectParameters.OrderBy(p => p.Name)) {
            GenerateParameterButton(projectParameter);
        }
    }

    private ProjectParameterButton GenerateParameterButton(ProjectParameter projectParameter) {
        ProjectParameterButton btn = Instantiate(ConstantButtonPrefab, ContentConstants.transform).GetComponent<ProjectParameterButton>();
        btn.Id = projectParameter.Id;
        btn.SetName(projectParameter.Name);
        btn.SetValue(Base.Parameter.GetValue<string>(projectParameter.Value));
        btn.Button.onClick.AddListener(async () => {
            if (!await EditConstantDialog.Init((_) => Show(), Show, projectParameter))
                return;
            Hide();
            EditConstantDialog.Open();
        });
        return btn;
    }


    private void DestroyConstantButtons() {
        RectTransform[] transforms = ContentConstants.GetComponentsInChildren<RectTransform>();
        if (transforms != null) {
            foreach (RectTransform o in transforms) {
                if (o.gameObject.tag != "Persistent") {
                    Destroy(o.gameObject);
                }
            }
        }
    }

    public async void ShowNewConstantDialog() {
        Hide();
        if (!await EditConstantDialog.Init((_) => Show(), Show))
            return;
        Hide();
        EditConstantDialog.Open();
    }

    #endregion

}
