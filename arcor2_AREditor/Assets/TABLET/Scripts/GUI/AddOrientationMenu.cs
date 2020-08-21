using Base;
using DanielLochner.Assets.SimpleSideMenu;
using IO.Swagger.Model;
using Michsky.UI.ModernUIPack;
using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(SimpleSideMenu))]
public class AddOrientationMenu : MonoBehaviour, IMenu {
    public Base.ActionPoint CurrentActionPoint;

    public TMPro.TMP_InputField NameInput;// QuaternionX, QuaternionY, QuaternionZ, QuaternionW;
    public DropdownParameter RobotsList, EndEffectorList;
    public GameObject LiteModeBlock, ManualModeBlock;
    public bool ManualMode;

    public OrientationManualEdit OrientationManualEdit;

    [SerializeField]
    private Button CreateNewOrientation;

    [SerializeField]
    private TooltipContent buttonTooltip;

    private SimpleSideMenu SideMenu;

    private void Start() {
        SideMenu = GetComponent<SimpleSideMenu>();
    }

  

    public void UpdateMenu() {
        CustomDropdown robotsListDropdown = RobotsList.Dropdown;
        robotsListDropdown.dropdownItems.Clear();

        RobotsList.gameObject.GetComponent<DropdownRobots>().Init(OnRobotChanged, true);
        if (robotsListDropdown.dropdownItems.Count > 0) {
            OnRobotChanged((string) RobotsList.GetValue());
        }

        ValidateFields();
    }

    /// <summary>
    /// updates EndEffectorList on selected robot change
    /// </summary>
    /// <param name="robot_name">Newly selected robot's name</param>
    private void OnRobotChanged(string robot_name) {
        EndEffectorList.Dropdown.dropdownItems.Clear();

        try {
            string robotId = SceneManager.Instance.RobotNameToId(robot_name);
            EndEffectorList.gameObject.GetComponent<DropdownEndEffectors>().Init(robotId, null);
        }
        catch (ItemNotFoundException ex) {
            Debug.LogError(ex);
            Notifications.Instance.ShowNotification("Failed to load end effectors", "");
        }

    }
    
    public async void ValidateFields() {
        bool interactable = true;
        name = NameInput.text;

        if (string.IsNullOrEmpty(name)) {
            buttonTooltip.description = "Name is required parameter";
            interactable = false;
        }
        else if (CurrentActionPoint.OrientationNameExist(name) || CurrentActionPoint.JointsNameExist(name)) {
            buttonTooltip.description = "There already exists orientation or joints with name " + name;
            interactable = false;
        }

        if (ManualMode) {
            if (interactable) {
                buttonTooltip.description = OrientationManualEdit.ValidateFields();
                if (!string.IsNullOrEmpty(buttonTooltip.description)) {
                    interactable = false;
                }
                /*if (string.IsNullOrEmpty(QuaternionX.text) || string.IsNullOrEmpty(QuaternionY.text) || string.IsNullOrEmpty(QuaternionZ.text) || string.IsNullOrEmpty(QuaternionW.text)) {
                    interactable = false;
                    buttonTooltip.description = "All quaternion values are required";
                }*/
            }
        }
        else {
            if (interactable) {
                if (RobotsList.Dropdown.dropdownItems.Count == 0) {
                    interactable = false;
                    buttonTooltip.description = "There is no robot to be used";
                }
            }
        }
        buttonTooltip.enabled = !interactable;
        CreateNewOrientation.interactable = interactable;
    }

    public async void AddOrientation() {
        Debug.Assert(CurrentActionPoint != null);


        string name = NameInput.text;
        try {

            if (ManualMode) {
                /*
                decimal x = decimal.Parse(QuaternionX.text, NumberStyles.AllowDecimalPoint | NumberStyles.AllowLeadingSign, CultureInfo.InvariantCulture);
                decimal y = decimal.Parse(QuaternionY.text, NumberStyles.AllowDecimalPoint | NumberStyles.AllowLeadingSign, CultureInfo.InvariantCulture);
                decimal z = decimal.Parse(QuaternionZ.text, NumberStyles.AllowDecimalPoint | NumberStyles.AllowLeadingSign, CultureInfo.InvariantCulture);
                decimal w = decimal.Parse(QuaternionW.text, NumberStyles.AllowDecimalPoint | NumberStyles.AllowLeadingSign, CultureInfo.InvariantCulture);
                IO.Swagger.Model.Orientation orientation = new IO.Swagger.Model.Orientation(w, x, y, z);
                */
                Orientation orientation = OrientationManualEdit.GetOrientation();
                await WebsocketManager.Instance.AddActionPointOrientation(CurrentActionPoint.Data.Id, orientation, name);
            } else { //using robot

                string robotId = SceneManager.Instance.RobotNameToId((string) RobotsList.GetValue());
                await WebsocketManager.Instance.AddActionPointOrientationUsingRobot(CurrentActionPoint.Data.Id, robotId, (string) EndEffectorList.GetValue(), name);
            }
            Close(); //close add menu

        } catch (ItemNotFoundException ex) {
            Notifications.Instance.ShowNotification("Failed to add new orientation", ex.Message);
        } catch (RequestFailedException ex) {
            Notifications.Instance.ShowNotification("Failed to add new orientation", ex.Message);
        }
    }

    public void ShowMenu(Base.ActionPoint actionPoint, bool manualMode) {
        ManualMode = manualMode;
        CurrentActionPoint = actionPoint;

        ManualModeBlock.SetActive(ManualMode);
        LiteModeBlock.SetActive(!ManualMode);

        NameInput.text = CurrentActionPoint.GetFreeOrientationName();
        /*
        QuaternionX.text = "0";
        QuaternionY.text = "0";
        QuaternionZ.text = "0";
        QuaternionW.text = "1";
        */
        OrientationManualEdit.SetOrientation(new Orientation());
        UpdateMenu();
        SideMenu.Open();
    }

    public void Close() {
        SideMenu.Close();
    }
}
