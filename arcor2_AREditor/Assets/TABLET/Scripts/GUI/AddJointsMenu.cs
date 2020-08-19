using System;
using Base;
using DanielLochner.Assets.SimpleSideMenu;
using Michsky.UI.ModernUIPack;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(SimpleSideMenu))]
public class AddJointsMenu : MonoBehaviour, IMenu {
    public Base.ActionPoint CurrentActionPoint;

    public TMPro.TMP_InputField NameInput;
    public DropdownParameter RobotsList;
    public GameObject ExpertModeBlock;


    [SerializeField]
    private Button CreateNewJoints;

    [SerializeField]
    private TooltipContent buttonTooltip;

    private SimpleSideMenu SideMenu;

    private bool ManualMode; //true for manual, false for using robot

    private void Start() {
        SideMenu = GetComponent<SimpleSideMenu>();
    }


    public void UpdateMenu() {
        CustomDropdown robotsListDropdown = RobotsList.Dropdown;
        robotsListDropdown.dropdownItems.Clear();

        RobotsList.gameObject.GetComponent<DropdownRobots>().Init(OnRobotChanged, false);
        if (robotsListDropdown.dropdownItems.Count > 0) {
            OnRobotChanged((string) RobotsList.GetValue());
        }

        ValidateFields();
    }

    private void OnRobotChanged(string robot_name) {
        //TODO: v expertmodu měnit seznam jointu
        if (!ManualMode)
            return;
        foreach (RectTransform o in ExpertModeBlock.GetComponentsInChildren<RectTransform>()) {
            if (!o.gameObject.CompareTag("Persistent")) {
                Destroy(o.gameObject);
            }
        }
        
        LabeledInput joint = Instantiate(GameManager.Instance.LabeledFloatInput, ExpertModeBlock.transform).GetComponent<LabeledInput>();
        joint.transform.localScale = new Vector3(1, 1, 1);
        joint.SetLabel("joint1", "jnt nr 1");
        joint.SetValue("55.66");
    }

    public async void ValidateFields() {
        bool interactable = true;
        name = NameInput.text;

        if (string.IsNullOrEmpty(name)) {
            buttonTooltip.description = "Name is required parameter";
            interactable = false;
        } else if (CurrentActionPoint.OrientationNameExist(name) || CurrentActionPoint.JointsNameExist(name)) {
            buttonTooltip.description = "There already exists orientation or joints with name " + name;
            interactable = false;
        }

        if (ManualMode) {
            if (interactable) {
                //TODO: check if all joints angles are filled - actually prolly needed to do this after when clicked create button
            }
        } else {
            if (interactable) {
                if (RobotsList.Dropdown.dropdownItems.Count == 0) {
                    interactable = false;
                    buttonTooltip.description = "There is no robot to be used";
                }
            }
        }
        buttonTooltip.enabled = !interactable;
        CreateNewJoints.interactable = interactable;
    }

    public async void AddJoints() {
        string robotName = (string) RobotsList.GetValue();

        IRobot robot;
        try {
            robot = SceneManager.Instance.GetRobotByName(robotName);
        } catch (ItemNotFoundException ex) {
            Notifications.Instance.ShowNotification("Failed to add joints", "Could not found robot called: " + robotName);
            Debug.LogError(ex);
            return;
        }

        Debug.Assert(CurrentActionPoint != null);
        try {
            await Base.WebsocketManager.Instance.AddActionPointJoints(CurrentActionPoint.Data.Id, robot.GetId(), name);
        } catch (RequestFailedException ex) {
            Notifications.Instance.ShowNotification("Failed to add action point", ex.Message);
            return;
        }
        Close();
        
    }

    /// <summary>
    /// Opens menu for adding joints
    /// </summary>
    /// <param name="actionPoint"></param>
    /// <param name="manual">true for manual mode, false for using robot</param>
    public void ShowMenu(Base.ActionPoint actionPoint, bool manual) {
        ManualMode = manual;
        CurrentActionPoint = actionPoint;
        ExpertModeBlock.SetActive(ManualMode);
        NameInput.text = CurrentActionPoint.GetFreeOrientationName();

        UpdateMenu();
        SideMenu.Open();
    }

    public void Close() {
        SideMenu.Close();
    }
}
