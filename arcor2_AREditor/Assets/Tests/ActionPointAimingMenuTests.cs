namespace TrilleonAutomation
{
    /*
    [AutomationClass]
    public class ActionPointAimingMenuTests : MonoBehaviour
    {
        private const int apiWaitingTime = 1; //how long should tests wait for response from server (in seconds)
        private int currentNumberOfOrientations = 1;
        private int currentNumberOfJoints = 1;

        private string NameOfProjectWithRobot = "testProject1";
        private string NameOfProjectWithoutRobot = "testProject2";

        Base.ActionPoint actionPoint;
        GameObject actionPointAimingMenu;
        private GameObject addOrientationMenu;
        private GameObject addJointsMenu;
        private GameObject orientationJointsDetailMenu;
        private GameObject confirmationDialog;
        GameObject focusButton;

        GameObject confirmationDialogOKButton;
        private GameObject backButton;
        private GameObject positionCollapsable;
        private GameObject orientationCollapsable;
        private GameObject jointsCollapsable;
        private GameObject positionManualEdit;
        private GameObject positionX;
        private GameObject positionY;
        private GameObject positionZ;
        private GameObject positionRobotList;
        private GameObject positionEEList;
        private GameObject positionUpdateUsingRobotBtn;
        private GameObject positionUpdateManualBtn;
        private GameObject positionExpertBlock;
        private GameObject positionRobotPickBlock;
        private GameObject orientationAddUsingRobotBtn;
        private GameObject orientationAddManualDefaultBtn;
        private GameObject orientationsDynamicList;
        private GameObject jointsAddUsingRobotBtn;
        private GameObject jointsDynamicList;
        private GameObject addOrientationMenuBackBtn;
        private GameObject addOrientationMenuNameInput;
        private GameObject addOrientationMenuRobotPickBlock;
        private GameObject addOrientationMenuExpertBlock;
        private GameObject addOrientationMenuCreateBtn;
        private GameObject addOrientationMenuQuaternionEulerSwitch;
        private GameObject addOrientationMenuXInput;
        private GameObject addOrientationMenuWInput;
        private GameObject addJointsMenuNameInput;
        private GameObject addJointsMenuCreateBtn;
        private GameObject addJointsMenuBackBtn;
        private GameObject detailMenuNameInput;
        private GameObject detailMenuBackBtn;
        private GameObject detailMenuDeleteBtn;
        private GameObject detailMenuOrientationBlock;
        private GameObject detailMenuJointsBlock;
        private GameObject detailMenuMoveHereBlock;
        private GameObject detailMenuOrientationExpertBlock;
        private GameObject detailMenuEditOrientationCollapsable;
        private GameObject detailMenuJointsExpertBlock;
        private GameObject detailMenuEditJointsCollapsable;
        private GameObject detailMenuUpdateBtn;
        private GameObject detailMenuOrientationManualSaveBtn;
        private GameObject detailMenuOrientationXInput;
        private GameObject detailMenuOrientationQuaternionEulerSwitch;
        private GameObject detailMenuJointsManualSaveBtn;
        private GameObject detailMenuJointsDynamicList;
        private GameObject detailMenuOrientationManualEdit;

        [SetUpClass]
        public IEnumerator SetUpClass()
        {
            focusButton = Q.driver.Find(By.Name, "Focus");
            actionPointAimingMenu = Q.driver.Find(By.Name, "ActionPointAimingMenu", false);
            addOrientationMenu = Q.driver.Find(By.Name, "AddOrientationMenu", false);
            addJointsMenu = Q.driver.Find(By.Name, "AddJointsMenu", false);
            orientationJointsDetailMenu = Q.driver.Find(By.Name, "OrientationJointsDetailMenu", false);

            EditorScreen editorScreen = Q.driver.Find(By.Name, "EditorScreen").GetComponent<EditorScreen>();
            confirmationDialog = Q.driver.FindIn(editorScreen, By.Name, "ConfirmationDialog");
            confirmationDialogOKButton = Q.driver.FindIn(confirmationDialog, By.Name, "Ok");
            backButton = Q.driver.FindIn(actionPointAimingMenu, By.Name, "BackBtn");

            positionCollapsable = Q.driver.Find(By.Name, "PositionCollapsableMenu");
            orientationCollapsable = Q.driver.Find(By.Name, "OrientationsCollapsableMenu");
            jointsCollapsable = Q.driver.Find(By.Name, "JointsCollapsableMenu");

            positionManualEdit = Q.driver.Find(By.Name, "PositionManualEdit");
            positionX = Q.driver.FindIn(positionCollapsable, By.Name, "X");
            positionY = Q.driver.FindIn(positionCollapsable, By.Name, "Y");
            positionZ = Q.driver.FindIn(positionCollapsable, By.Name, "Z");
            positionRobotList = Q.driver.Find(By.Name, "PositionRobotsList");
            positionEEList = Q.driver.FindIn(positionCollapsable, By.Name, "EndEffectorList");
            positionUpdateUsingRobotBtn = Q.driver.Find(By.Name, "UpdatePositionUsingRobotButton");
            positionUpdateManualBtn = Q.driver.Find(By.Name, "UpdatePositionManualButton");
            positionExpertBlock = Q.driver.FindIn(positionCollapsable, By.Name, "ExpertModeBlock");
            positionRobotPickBlock = Q.driver.FindIn(positionCollapsable, By.Name, "RobotPickBlock");

            orientationAddUsingRobotBtn = Q.driver.FindIn(orientationCollapsable, By.Name, "AddUsingRobotButton");
            orientationAddManualDefaultBtn = Q.driver.FindIn(orientationCollapsable, By.Name, "ManualDefaultButton");
            orientationsDynamicList = Q.driver.FindIn(orientationCollapsable, By.Name, "OrientationsDynamicList");

            jointsAddUsingRobotBtn = Q.driver.FindIn(jointsCollapsable, By.Name, "AddUsingRobotButton");
            jointsDynamicList = Q.driver.FindIn(jointsCollapsable, By.Name, "JointsDynamicList");

            
            addOrientationMenuBackBtn = Q.driver.FindIn(addOrientationMenu, By.Name, "BackBtn");
            addOrientationMenuNameInput = Q.driver.FindIn(addOrientationMenu, By.Name, "Input");
            addOrientationMenuRobotPickBlock = Q.driver.FindIn(addOrientationMenu, By.Name, "RobotSelectionBlock");
            addOrientationMenuExpertBlock = Q.driver.FindIn(addOrientationMenu, By.Name, "ExpertModeBlock");
            addOrientationMenuCreateBtn = Q.driver.FindIn(addOrientationMenu, By.Name, "CreateOrientation");
            addOrientationMenuQuaternionEulerSwitch = Q.driver.FindIn(addOrientationMenu, By.Name, "switch");
            addOrientationMenuXInput = Q.driver.FindIn(addOrientationMenu, By.Name, "X");
            addOrientationMenuWInput = Q.driver.FindIn(addOrientationMenu, By.Name, "W");


            addJointsMenuNameInput = Q.driver.FindIn(addJointsMenu, By.Name, "Input");
            addJointsMenuCreateBtn = Q.driver.FindIn(addJointsMenu, By.Name, "CreateJoints");
            addJointsMenuBackBtn = Q.driver.FindIn(addJointsMenu, By.Name, "BackBtn");


            detailMenuNameInput = Q.driver.FindIn(orientationJointsDetailMenu, By.Name, "NameInput");
            detailMenuBackBtn = Q.driver.FindIn(orientationJointsDetailMenu, By.Name, "BackBtn");
            detailMenuDeleteBtn = Q.driver.FindIn(orientationJointsDetailMenu, By.Name, "Remove");
            detailMenuOrientationBlock = Q.driver.FindIn(orientationJointsDetailMenu, By.Name, "OrientationBlock");
            detailMenuJointsBlock = Q.driver.FindIn(orientationJointsDetailMenu, By.Name, "JointsBlock");
            detailMenuMoveHereBlock = Q.driver.FindIn(orientationJointsDetailMenu, By.Name, "MoveHereBlock");
            detailMenuOrientationExpertBlock = Q.driver.FindIn(orientationJointsDetailMenu, By.Name, "OrientationExpertModeBlock");
            detailMenuEditOrientationCollapsable = Q.driver.FindIn(orientationJointsDetailMenu, By.Name, "EditOrientationCollapsableMenu");
            detailMenuJointsExpertBlock = Q.driver.FindIn(orientationJointsDetailMenu, By.Name, "JointsExpertModeBlock");
            detailMenuEditJointsCollapsable = Q.driver.FindIn(orientationJointsDetailMenu, By.Name, "EditJointsCollapsableMenu");
            detailMenuUpdateBtn = Q.driver.FindIn(orientationJointsDetailMenu, By.Name, "UpdateActionButton");
            detailMenuOrientationManualSaveBtn = Q.driver.FindIn(detailMenuOrientationExpertBlock, By.Name, "SaveActionButton");
            detailMenuOrientationXInput = Q.driver.FindIn(detailMenuOrientationExpertBlock, By.Name, "X");
            detailMenuOrientationQuaternionEulerSwitch = Q.driver.FindIn(detailMenuOrientationExpertBlock, By.Name, "switch");
            detailMenuJointsManualSaveBtn = Q.driver.FindIn(detailMenuJointsExpertBlock, By.Name, "SaveActionButton");
            detailMenuJointsDynamicList = Q.driver.FindIn(orientationJointsDetailMenu, By.Name, "JointsDynamicList");
            detailMenuOrientationManualEdit = Q.driver.FindIn(orientationJointsDetailMenu, By.Name, "OrientationManualEdit");


            yield return null;
        }

        [SetUp]
        public IEnumerator SetUp()
        {
            
            yield return null;
        }

        [DependencyTest(1)]
        [Automation("Action point aiming menu tests")]
        public IEnumerator AimingMenuExpertModeTest()
        {
            foreach (IO.Swagger.Model.ListProjectsResponseData project in GameManager.Instance.Projects) {
                if (project.Name == NameOfProjectWithRobot) {
                    GameManager.Instance.OpenProject(project.Id);
                    break;
                }
            }

            GameManager.Instance.ExpertMode = true;
            yield return new WaitForSeconds(4); //wait for project to load
            WebsocketManager.Instance.StartScene(false);
            yield return new WaitForSeconds(apiWaitingTime);

            actionPoint = ProjectManager.Instance.GetactionpointByName("sph_ap");
            actionPoint.OnClick(Clickable.Click.MOUSE_RIGHT_BUTTON);

            yield return StartCoroutine(Q.driver.Click(focusButton, "Click on aiming menu icon."));
            yield return StartCoroutine(Q.assert.IsTrue(MenuManager.Instance.ActionPointAimingMenu.CurrentState == SimpleSideMenu.State.Open, "Aiming menu should be opened"));

            yield return StartCoroutine(Q.assert.IsTrue(positionCollapsable.gameObject.activeSelf, "Position collapsable menu should be active."));
            yield return StartCoroutine(Q.assert.IsTrue(orientationCollapsable.gameObject.activeSelf, "Orientations collapsable menu should be active."));
            yield return StartCoroutine(Q.assert.IsTrue(jointsCollapsable.gameObject.activeSelf, "Joints collapsable menu should be active."));
            yield return StartCoroutine(Q.driver.Click(positionCollapsable, "Click on position collapsable menu."));
            yield return StartCoroutine(Q.driver.Click(orientationCollapsable, "Click on orientations collapsable menu."));
            yield return StartCoroutine(Q.driver.Click(jointsCollapsable, "Click on joints collapsable menu."));


            //position collapsable menu tests
            yield return StartCoroutine(Q.assert.IsTrue(positionRobotPickBlock.gameObject.activeSelf, "Update robot pick block should be active"));
            yield return StartCoroutine(Q.assert.IsTrue(positionExpertBlock.gameObject.activeSelf, "Update position manually block should be active"));
            yield return StartCoroutine(Q.assert.IsTrue(positionRobotList.gameObject.activeSelf, "Position robots list should be active"));
            yield return StartCoroutine(Q.assert.IsTrue(positionEEList.gameObject.activeSelf, "Position end effector list should be active"));
            yield return StartCoroutine(Q.assert.IsTrue(positionRobotList.GetComponent<DropdownParameter>().Dropdown.dropdownItems.Count == 1, "In position robots list should be one item"));
            yield return StartCoroutine(Q.assert.IsTrue(positionEEList.GetComponent<DropdownParameter>().Dropdown.dropdownItems.Count >= 1, "In position end effector list should be at least one item"));

            yield return StartCoroutine(Q.assert.IsTrue(positionManualEdit.gameObject.activeSelf, "Position manual editing should be active"));
            Position apPosition = positionManualEdit.GetComponent<PositionManualEdit>().GetPosition();
            yield return StartCoroutine(Q.assert.IsTrue(apPosition.X == 0m && apPosition.Y == 0m && apPosition.Z == 0.2m, "Position values should be 0;0;0.2"));
            //edit position manually
            positionX.GetComponent<TMP_InputField>().text = "0.3";
            positionY.GetComponent<TMP_InputField>().text = "-0.2";
            positionZ.GetComponent<TMP_InputField>().text = "0.15";
            yield return StartCoroutine(Q.driver.Click(positionUpdateManualBtn, "Click on update position manually."));
            yield return new WaitForSeconds(apiWaitingTime);
            var newPosition = actionPoint.Data.Position;
            yield return StartCoroutine(Q.assert.IsTrue(newPosition.X == 0.3m && newPosition.Y == -0.2m && newPosition.Z == 0.15m, "Position of action point is not set right"));
            //edit position using robot
            yield return StartCoroutine(Q.driver.Click(positionUpdateUsingRobotBtn, "Click on update position using robot."));
            yield return StartCoroutine(Q.driver.Click(confirmationDialogOKButton, "Click on ok button"));
            yield return new WaitForSeconds(apiWaitingTime);
            newPosition = actionPoint.Data.Position;
            yield return StartCoroutine(Q.assert.IsTrue(!(newPosition.X == 0.3m && newPosition.Y == -0.2m && newPosition.Z == 0.15m), "Position of action point after updating using robot hasn't changed"));


            //orientation collapsable menu tests
            yield return StartCoroutine(Q.assert.IsTrue(orientationAddUsingRobotBtn.gameObject.activeSelf, "Add orientation using robot button should be active"));
            yield return StartCoroutine(Q.assert.IsTrue(orientationAddUsingRobotBtn.GetComponent<Button>().interactable, "Add orientation using robot button should be interactable"));
            yield return StartCoroutine(Q.assert.IsTrue(orientationAddManualDefaultBtn.gameObject.activeSelf, "Add orientation manually button should be active"));
            yield return StartCoroutine(Q.assert.IsTrue(orientationAddManualDefaultBtn.GetComponent<ActionButton>().GetLabel() == "Manual", "Label of right button in orientation add section should be 'Manual'"));
            yield return StartCoroutine(Q.assert.IsTrue(orientationsDynamicList.transform.childCount == currentNumberOfOrientations, "There should be one orientation in orientations list"));


            //joints collapsable menu tests
            yield return StartCoroutine(Q.assert.IsTrue(jointsAddUsingRobotBtn.gameObject.activeSelf, "Add joints using robot button should be active"));
            yield return StartCoroutine(Q.assert.IsTrue(jointsDynamicList.transform.childCount == currentNumberOfJoints, "There should be one joints in joints list"));


            //close aiming menu
            yield return StartCoroutine(Q.driver.Click(backButton, "Click on back button"));
            yield return StartCoroutine(Q.assert.IsTrue(MenuManager.Instance.ActionPointAimingMenu.CurrentState == SimpleSideMenu.State.Closed, "Aiming menu should be closed"));
        }

        [DependencyTest(2)]
        [Automation("Action point aiming menu tests")]
        public IEnumerator AimingMenuLiteModeTest() {
            GameManager.Instance.ExpertMode = false;

            yield return StartCoroutine(Q.driver.Click(focusButton, "Click on aiming menu icon."));
            yield return StartCoroutine(Q.assert.IsTrue(MenuManager.Instance.ActionPointAimingMenu.CurrentState == SimpleSideMenu.State.Open, "Aiming menu should be opened"));

            yield return StartCoroutine(Q.assert.IsTrue(positionCollapsable.gameObject.activeSelf, "Position collapsable menu should be active."));
            yield return StartCoroutine(Q.assert.IsTrue(orientationCollapsable.gameObject.activeSelf, "Orientations collapsable menu should be active."));
            yield return StartCoroutine(Q.assert.IsTrue(jointsCollapsable.gameObject.activeSelf, "Joints collapsable menu should be active."));


            //position collapsable menu tests
            yield return StartCoroutine(Q.assert.IsTrue(positionRobotPickBlock.gameObject.activeSelf, "Update robot pick block should be active"));
            yield return StartCoroutine(Q.assert.IsTrue(!positionExpertBlock.gameObject.activeSelf, "Update position manually block should not be active"));
            yield return StartCoroutine(Q.assert.IsTrue(positionRobotList.GetComponent<DropdownParameter>().Dropdown.dropdownItems.Count == 1, "In position robots list should be one item"));
            yield return StartCoroutine(Q.assert.IsTrue(positionEEList.GetComponent<DropdownParameter>().Dropdown.dropdownItems.Count >= 1, "In position end effector list should be at least one item"));


            //orientation collapsable menu tests
            yield return StartCoroutine(Q.assert.IsTrue(orientationAddUsingRobotBtn.gameObject.activeSelf, "Add orientation using robot button should be active"));
            yield return StartCoroutine(Q.assert.IsTrue(orientationAddManualDefaultBtn.gameObject.activeSelf, "Add default orientation button should be active"));
            yield return StartCoroutine(Q.assert.IsTrue(orientationAddManualDefaultBtn.GetComponent<ActionButton>().GetLabel() == "Default", "Label of right button in orientation add section should be 'Default'"));
            yield return StartCoroutine(Q.assert.IsTrue(orientationsDynamicList.transform.childCount == currentNumberOfOrientations, "There should be " + currentNumberOfOrientations + " orientation in orientations list"));


            //joints collapsable menu tests
            yield return StartCoroutine(Q.assert.IsTrue(jointsAddUsingRobotBtn.gameObject.activeSelf, "Add joints using robot button should be active"));
            yield return StartCoroutine(Q.assert.IsTrue(jointsDynamicList.transform.childCount == currentNumberOfJoints, "There should be " + currentNumberOfJoints + " joints in joints list"));


            //close aiming menu
            yield return StartCoroutine(Q.driver.Click(backButton, "Click on back button"));
            yield return StartCoroutine(Q.assert.IsTrue(MenuManager.Instance.ActionPointAimingMenu.CurrentState == SimpleSideMenu.State.Closed, "Aiming menu should be closed"));
        }

        [DependencyTest(3)]
        [Automation("Action point aiming menu tests")]
        public IEnumerator AddOrientationLiteModeTest() {
            GameManager.Instance.ExpertMode = false;

            //open aiming menu
            yield return StartCoroutine(Q.driver.Click(focusButton, "Click on aiming menu icon."));
            yield return StartCoroutine(Q.assert.IsTrue(MenuManager.Instance.ActionPointAimingMenu.CurrentState == SimpleSideMenu.State.Open, "Aiming menu should be opened"));


            //add default
            yield return StartCoroutine(Q.driver.Click(orientationAddManualDefaultBtn, "Click on add default orientation."));
            yield return new WaitForSeconds(apiWaitingTime);
            yield return StartCoroutine(Q.assert.IsTrue(orientationsDynamicList.transform.childCount == ++currentNumberOfOrientations, "There should be " + currentNumberOfOrientations + " orientations in list now"));

            //add using robot
            yield return StartCoroutine(Q.driver.Click(orientationAddUsingRobotBtn, "Click on add orientation using robot."));
            yield return StartCoroutine(Q.assert.IsTrue(MenuManager.Instance.AddOrientationMenu.CurrentState == SimpleSideMenu.State.Open, "Add orientation menu should be opened"));
            yield return StartCoroutine(Q.assert.IsTrue(!addOrientationMenuExpertBlock.gameObject.activeSelf, "Expert mode block should not be active"));
            yield return StartCoroutine(Q.assert.IsTrue(addOrientationMenuRobotPickBlock.gameObject.activeSelf, "Robot selection block should be active"));
            string orientationUsingRobotName = "ori_using_robot";
            addOrientationMenuNameInput.GetComponent<TMP_InputField>().text = orientationUsingRobotName;
            yield return StartCoroutine(Q.driver.Click(addOrientationMenuCreateBtn, "Click on ok button (create orientation using robot)."));
            yield return new WaitForSeconds(apiWaitingTime);
            yield return StartCoroutine(Q.assert.IsTrue(MenuManager.Instance.AddOrientationMenu.CurrentState == SimpleSideMenu.State.Closed, "Add orientation menu should be closed"));
            yield return StartCoroutine(Q.assert.IsTrue(orientationsDynamicList.transform.childCount == ++currentNumberOfOrientations, "There should be " + currentNumberOfOrientations + " orientations in list now"));
            NamedOrientation usingRobotOrientation = null;
            try {
                usingRobotOrientation = actionPoint.GetOrientationByName(orientationUsingRobotName);
            } catch (KeyNotFoundException ex) {
                usingRobotOrientation = null;
            }
            yield return StartCoroutine(Q.assert.IsTrue(!usingRobotOrientation.IsNull(), "Orientation not found by name!"));


            //test back button
            yield return StartCoroutine(Q.driver.Click(orientationAddUsingRobotBtn, "Click on add orientation using robot."));
            yield return StartCoroutine(Q.assert.IsTrue(MenuManager.Instance.AddOrientationMenu.CurrentState == SimpleSideMenu.State.Open, "Add orientation menu should be opened"));
            addOrientationMenuNameInput.GetComponent<TMP_InputField>().text = orientationUsingRobotName;
            yield return StartCoroutine(Q.assert.IsTrue(!addOrientationMenuCreateBtn.GetComponent<Button>().interactable, "Ok button should not be interactable (existing name)"));
            yield return StartCoroutine(Q.driver.Click(addOrientationMenuBackBtn, "Click on back button (close add orientation menu)."));
            yield return StartCoroutine(Q.assert.IsTrue(MenuManager.Instance.AddOrientationMenu.CurrentState == SimpleSideMenu.State.Closed, "Add orientation menu should be closed"));
            yield return StartCoroutine(Q.assert.IsTrue(orientationsDynamicList.transform.childCount == currentNumberOfOrientations, "There should be " + currentNumberOfOrientations + " orientations in list"));


            //close aiming menu
            yield return StartCoroutine(Q.driver.Click(backButton, "Click on back button"));
            yield return StartCoroutine(Q.assert.IsTrue(MenuManager.Instance.ActionPointAimingMenu.CurrentState == SimpleSideMenu.State.Closed, "Aiming menu should be closed"));
        }


        [DependencyTest(4)]
        [Automation("Action point aiming menu tests")]
        public IEnumerator AddOrientationExpertModeTest() {
            GameManager.Instance.ExpertMode = true;

            //open aiming menu
            yield return StartCoroutine(Q.driver.Click(focusButton, "Click on aiming menu icon."));
            yield return StartCoroutine(Q.assert.IsTrue(MenuManager.Instance.ActionPointAimingMenu.CurrentState == SimpleSideMenu.State.Open, "Aiming menu should be opened"));


            //add manual
            yield return StartCoroutine(Q.driver.Click(orientationAddManualDefaultBtn, "Click on add manual orientation."));
            yield return StartCoroutine(Q.assert.IsTrue(MenuManager.Instance.AddOrientationMenu.CurrentState == SimpleSideMenu.State.Open, "Add orientation menu should be opened"));
            yield return StartCoroutine(Q.assert.IsTrue(addOrientationMenuExpertBlock.gameObject.activeSelf, "Expert mode block should  be active"));
            yield return StartCoroutine(Q.assert.IsTrue(!addOrientationMenuRobotPickBlock.gameObject.activeSelf, "Robot selection block should not be active"));
            string orientationName = "ori_manual";
            addOrientationMenuNameInput.GetComponent<TMP_InputField>().text = orientationName;

            yield return StartCoroutine(Q.driver.Click(addOrientationMenuQuaternionEulerSwitch, "Click on switch between quaternion and euler angles."));
            yield return StartCoroutine(Q.assert.IsTrue(!addOrientationMenuWInput.gameObject.activeSelf, "W input should not be active"));
            yield return StartCoroutine(Q.driver.Click(addOrientationMenuQuaternionEulerSwitch, "Click on switch between quaternion and euler angles."));
            yield return StartCoroutine(Q.assert.IsTrue(addOrientationMenuWInput.gameObject.activeSelf, "W input should be active"));

            addOrientationMenuXInput.GetComponent<TMP_InputField>().text = "0.5";
            addOrientationMenuWInput.GetComponent<TMP_InputField>().text = "1";
            yield return StartCoroutine(Q.driver.Click(addOrientationMenuCreateBtn, "Click on ok button (create orientation using robot)."));
            yield return new WaitForSeconds(apiWaitingTime);
            yield return StartCoroutine(Q.assert.IsTrue(MenuManager.Instance.AddOrientationMenu.CurrentState == SimpleSideMenu.State.Closed, "Add orientation menu should be closed"));
            yield return StartCoroutine(Q.assert.IsTrue(orientationsDynamicList.transform.childCount == ++currentNumberOfOrientations, "There should be " + currentNumberOfOrientations + " orientations in list now"));
            NamedOrientation orientation;
            try {
                orientation = actionPoint.GetOrientationByName(orientationName);
            } catch (KeyNotFoundException ex) {
                orientation = null;
            }
            yield return StartCoroutine(Q.assert.IsTrue(!orientation.IsNull(), "Orientation not found by name!"));
            yield return StartCoroutine(Q.assert.IsTrue(orientation.Orientation.X - 0.4472m < 0.001m, "Orientation saved with wrong value"));
            yield return StartCoroutine(Q.assert.IsTrue(orientation.Orientation.W - 0.8944m < 0.001m, "Orientation saved with wrong value"));


            //close aiming menu
            yield return StartCoroutine(Q.driver.Click(backButton, "Click on back button"));
            yield return StartCoroutine(Q.assert.IsTrue(MenuManager.Instance.ActionPointAimingMenu.CurrentState == SimpleSideMenu.State.Closed, "Aiming menu should be closed"));
        }



        [DependencyTest(5)]
        [Automation("Action point aiming menu tests")]
        public IEnumerator AddJointsTest() {
            GameManager.Instance.ExpertMode = true;

            //open aiming menu
            yield return StartCoroutine(Q.driver.Click(focusButton, "Click on aiming menu icon."));
            yield return StartCoroutine(Q.assert.IsTrue(MenuManager.Instance.ActionPointAimingMenu.CurrentState == SimpleSideMenu.State.Open, "Aiming menu should be opened"));


            //add
            string jointsName = "test_joints";
            yield return StartCoroutine(Q.driver.Click(jointsAddUsingRobotBtn, "Click on add joints."));
            yield return StartCoroutine(Q.assert.IsTrue(MenuManager.Instance.AddJointsMenu.CurrentState == SimpleSideMenu.State.Open, "Add joints menu should be opened"));
            yield return StartCoroutine(Q.assert.IsTrue(addJointsMenuNameInput.gameObject.activeSelf, "Name input should be active"));
            addJointsMenuNameInput.GetComponent<TMP_InputField>().text = "";
            yield return StartCoroutine(Q.assert.IsTrue(!addJointsMenuCreateBtn.GetComponent<Button>().interactable, "Ok button should not be interactable (empty name)"));
            addJointsMenuNameInput.GetComponent<TMP_InputField>().text = jointsName;
            yield return StartCoroutine(Q.assert.IsTrue(addJointsMenuCreateBtn.GetComponent<Button>().interactable, "Ok button should be interactable"));
            yield return StartCoroutine(Q.driver.Click(addJointsMenuCreateBtn, "Click on ok button (create orientation using robot)."));
            yield return new WaitForSeconds(apiWaitingTime);
            yield return StartCoroutine(Q.assert.IsTrue(MenuManager.Instance.AddJointsMenu.CurrentState == SimpleSideMenu.State.Closed, "Add joints menu should be closed"));
            yield return StartCoroutine(Q.assert.IsTrue(jointsDynamicList.transform.childCount == ++currentNumberOfJoints, "There should be " + currentNumberOfJoints + " joints in list now"));
            ProjectRobotJoints joints;
            try {
                joints = actionPoint.GetJointsByName(jointsName);
            } catch (KeyNotFoundException ex) {
                joints = null;
            }
            yield return StartCoroutine(Q.assert.IsTrue(!joints.IsNull(), "Joints not found by name!"));


            //back btn
            yield return StartCoroutine(Q.driver.Click(jointsAddUsingRobotBtn, "Click on add joints."));
            yield return StartCoroutine(Q.assert.IsTrue(MenuManager.Instance.AddJointsMenu.CurrentState == SimpleSideMenu.State.Open, "Add joints menu should be opened"));
            addJointsMenuNameInput.GetComponent<TMP_InputField>().text = jointsName;
            yield return StartCoroutine(Q.assert.IsTrue(!addJointsMenuCreateBtn.GetComponent<Button>().interactable, "Ok button should not be interactable (existing name)"));
            yield return StartCoroutine(Q.driver.Click(addJointsMenuBackBtn, "Click on back button"));
            yield return StartCoroutine(Q.assert.IsTrue(MenuManager.Instance.AddJointsMenu.CurrentState == SimpleSideMenu.State.Closed, "Add joints menu should be closed"));


            //close aiming menu
            yield return StartCoroutine(Q.driver.Click(backButton, "Click on back button"));
            yield return StartCoroutine(Q.assert.IsTrue(MenuManager.Instance.ActionPointAimingMenu.CurrentState == SimpleSideMenu.State.Closed, "Aiming menu should be closed"));
        }


        [DependencyTest(6)]
        [Automation("Action point aiming menu tests")]
        public IEnumerator OrientationDetailMenuExpertModeTest() {
            GameManager.Instance.ExpertMode = true;

            //open aiming menu
            yield return StartCoroutine(Q.driver.Click(focusButton, "Click on aiming menu icon."));
            yield return StartCoroutine(Q.assert.IsTrue(MenuManager.Instance.ActionPointAimingMenu.CurrentState == SimpleSideMenu.State.Open, "Aiming menu should be opened"));


            //open "default" orientation
            ActionButton orientationBtn = null;
            ActionButton[] orientationsBtns = orientationsDynamicList.GetComponentsInChildren<ActionButton>();
            foreach (ActionButton btn in orientationsBtns) {
                if (btn.GetLabel() == "default") {
                    orientationBtn = btn;
                    break;
                }
            }
            yield return StartCoroutine(Q.driver.Click(orientationBtn, "Click on default orientation."));
            yield return StartCoroutine(Q.assert.IsTrue(MenuManager.Instance.OrientationJointsDetailMenu.CurrentState == SimpleSideMenu.State.Open, "Orientation detail menu should be opened"));


            yield return StartCoroutine(Q.assert.IsTrue(detailMenuNameInput.GetComponent<TMP_InputField>().text == "default", "Orientation name should be \"default\""));
            yield return StartCoroutine(Q.assert.IsTrue(detailMenuOrientationBlock.gameObject.activeSelf, "Orientation block should be active"));
            yield return StartCoroutine(Q.assert.IsTrue(detailMenuOrientationExpertBlock.gameObject.activeSelf, "Orientation expert block should be active"));
            yield return StartCoroutine(Q.assert.IsTrue(!detailMenuJointsBlock.gameObject.activeSelf, "Joints block should not be active"));
            yield return StartCoroutine(Q.assert.IsTrue(!detailMenuJointsExpertBlock.gameObject.activeSelf, "Joints expert block should not be active"));
            yield return StartCoroutine(Q.assert.IsTrue(detailMenuMoveHereBlock.gameObject.activeSelf, "Move here block should be active"));
            yield return StartCoroutine(Q.assert.IsTrue(detailMenuUpdateBtn.GetComponent<Button>().interactable, "Update button should be interactable"));
            yield return StartCoroutine(Q.assert.IsTrue(detailMenuOrientationManualEdit.GetComponent<OrientationManualEdit>().GetOrientation().Equals(new Orientation()), "Orientation should be 0,0,0,1"));


            //edit orientation manually (euler angles)
            yield return StartCoroutine(Q.driver.Click(detailMenuEditOrientationCollapsable, "Click on edit orientation collapsable."));
            yield return StartCoroutine(Q.driver.Click(detailMenuOrientationQuaternionEulerSwitch, "Click on switch to euler angles."));
            detailMenuOrientationXInput.GetComponent<TMP_InputField>().text = "50";
            yield return StartCoroutine(Q.driver.Click(detailMenuOrientationManualSaveBtn, "Click on save euler angles."));
            yield return new WaitForSeconds(apiWaitingTime);
            NamedOrientation orientation = actionPoint.GetNamedOrientationByName("default");
            yield return StartCoroutine(Q.assert.IsTrue(orientation.Orientation.X - 0.4226m < 0.001m, "Orientation saved with wrong value"));
            yield return StartCoroutine(Q.assert.IsTrue(orientation.Orientation.W - 0.9063m < 0.001m, "Orientation saved with wrong value"));


            //close
            yield return StartCoroutine(Q.driver.Click(detailMenuBackBtn, "Click on back button (close detail menu)"));
            yield return StartCoroutine(Q.assert.IsTrue(MenuManager.Instance.OrientationJointsDetailMenu.CurrentState == SimpleSideMenu.State.Closed, "Orientation detail menu should be closed"));

            //close aiming menu
            yield return StartCoroutine(Q.driver.Click(backButton, "Click on back button"));
            yield return StartCoroutine(Q.assert.IsTrue(MenuManager.Instance.ActionPointAimingMenu.CurrentState == SimpleSideMenu.State.Closed, "Aiming menu should be closed"));
        }

        [DependencyTest(7)]
        [Automation("Action point aiming menu tests")]
        public IEnumerator OrientationDetailMenuLiteModeTest() {
            GameManager.Instance.ExpertMode = false;

            //open aiming menu
            yield return StartCoroutine(Q.driver.Click(focusButton, "Click on aiming menu icon."));
            yield return StartCoroutine(Q.assert.IsTrue(MenuManager.Instance.ActionPointAimingMenu.CurrentState == SimpleSideMenu.State.Open, "Aiming menu should be opened"));


            //open "default" orientation
            ActionButton orientationBtn = null;
            ActionButton[] orientationsBtns = orientationsDynamicList.GetComponentsInChildren<ActionButton>();
            foreach (ActionButton btn in orientationsBtns) {
                if (btn.GetLabel() == "default") {
                    orientationBtn = btn;
                    break;
                }
            }
            yield return StartCoroutine(Q.driver.Click(orientationBtn, "Click on default orientation."));
            yield return StartCoroutine(Q.assert.IsTrue(MenuManager.Instance.OrientationJointsDetailMenu.CurrentState == SimpleSideMenu.State.Open, "Orientation detail menu should be opened"));


            yield return StartCoroutine(Q.assert.IsTrue(detailMenuNameInput.GetComponent<TMP_InputField>().text == "default", "Orientation name should be \"default\""));
            yield return StartCoroutine(Q.assert.IsTrue(detailMenuOrientationBlock.gameObject.activeSelf, "Orientation block should be active"));
            yield return StartCoroutine(Q.assert.IsTrue(!detailMenuOrientationExpertBlock.gameObject.activeSelf, "Orientation expert block should not be active"));
            yield return StartCoroutine(Q.assert.IsTrue(!detailMenuJointsBlock.gameObject.activeSelf, "Joints block should not be active"));
            yield return StartCoroutine(Q.assert.IsTrue(!detailMenuJointsExpertBlock.gameObject.activeSelf, "Joints expert block should not be active"));
            yield return StartCoroutine(Q.assert.IsTrue(detailMenuMoveHereBlock.gameObject.activeSelf, "Move here block should be active"));
            yield return StartCoroutine(Q.assert.IsTrue(detailMenuUpdateBtn.GetComponent<Button>().interactable, "Update button should be interactable"));


            //rename test
            detailMenuNameInput.GetComponent<TMP_InputField>().text = "new_name";
            detailMenuNameInput.GetComponent<TMP_InputField>().SendMessage("SendOnEndEdit");
            yield return new WaitForSeconds(apiWaitingTime);
            NamedOrientation orientation;
            try {
                orientation = actionPoint.GetOrientationByName("new_name");
            } catch (KeyNotFoundException ex) {
                orientation = null;
            }
            yield return StartCoroutine(Q.assert.IsTrue(!orientation.IsNull(), "Orientation name should be changed to \"new_name\"."));


            //update test
            Orientation backup = new Orientation(orientation.Orientation.W, orientation.Orientation.X, orientation.Orientation.Y, orientation.Orientation.Z);
            yield return StartCoroutine(Q.driver.Click(detailMenuUpdateBtn, "Click on update button."));
            yield return StartCoroutine(Q.driver.Click(confirmationDialogOKButton, "Click on ok button."));
            yield return new WaitForSeconds(apiWaitingTime);
            yield return StartCoroutine(Q.assert.IsTrue(!actionPoint.GetNamedOrientationByName("new_name").Orientation.Equals(backup), "Orientation should have changed after update using robot"));

            //delete
            yield return StartCoroutine(Q.driver.Click(detailMenuDeleteBtn, "Click on delete button"));
            yield return StartCoroutine(Q.driver.Click(confirmationDialogOKButton, "Click on ok button."));
            yield return new WaitForSeconds(apiWaitingTime);
            yield return StartCoroutine(Q.assert.IsTrue(MenuManager.Instance.OrientationJointsDetailMenu.CurrentState == SimpleSideMenu.State.Closed, "Orientation detail menu should be closed"));
            yield return StartCoroutine(Q.assert.IsTrue(orientationsDynamicList.transform.childCount == --currentNumberOfOrientations, "There should be " + currentNumberOfOrientations + " orientation in orientations list"));


            //close aiming menu
            yield return StartCoroutine(Q.driver.Click(backButton, "Click on back button"));
            yield return StartCoroutine(Q.assert.IsTrue(MenuManager.Instance.ActionPointAimingMenu.CurrentState == SimpleSideMenu.State.Closed, "Aiming menu should be closed"));
        }


        [DependencyTest(8)]
        [Automation("Action point aiming menu tests")]
        public IEnumerator JointsDetailMenuLiteModeTest() {
            GameManager.Instance.ExpertMode = false;

            //open aiming menu
            yield return StartCoroutine(Q.driver.Click(focusButton, "Click on aiming menu icon."));
            yield return StartCoroutine(Q.assert.IsTrue(MenuManager.Instance.ActionPointAimingMenu.CurrentState == SimpleSideMenu.State.Open, "Aiming menu should be opened"));


            //open "default_1" joints
            ActionButton jointsBtn = null;
            ActionButton[] jointsBtns = jointsDynamicList.GetComponentsInChildren<ActionButton>();
            foreach (ActionButton btn in jointsBtns) {
                if (btn.GetLabel() == "default_1") {
                    jointsBtn = btn;
                    break;
                }
            }
            yield return StartCoroutine(Q.driver.Click(jointsBtn, "Click on default_1 joints."));
            yield return StartCoroutine(Q.assert.IsTrue(MenuManager.Instance.OrientationJointsDetailMenu.CurrentState == SimpleSideMenu.State.Open, "Joints detail menu should be opened"));


            yield return StartCoroutine(Q.assert.IsTrue(detailMenuNameInput.GetComponent<TMP_InputField>().text == "default_1", "Joints name should be \"default_1\""));
            yield return StartCoroutine(Q.assert.IsTrue(!detailMenuOrientationBlock.gameObject.activeSelf, "Orientation block should not be active"));
            yield return StartCoroutine(Q.assert.IsTrue(!detailMenuOrientationExpertBlock.gameObject.activeSelf, "Orientation expert block should not be active"));
            yield return StartCoroutine(Q.assert.IsTrue(detailMenuJointsBlock.gameObject.activeSelf, "Joints block should be active"));
            yield return StartCoroutine(Q.assert.IsTrue(!detailMenuJointsExpertBlock.gameObject.activeSelf, "Joints expert block should not be active"));
            yield return StartCoroutine(Q.assert.IsTrue(detailMenuMoveHereBlock.gameObject.activeSelf, "Move here block should be active"));
            yield return StartCoroutine(Q.assert.IsTrue(detailMenuUpdateBtn.GetComponent<Button>().interactable, "Update button should be interactable"));


            //rename test
            detailMenuNameInput.GetComponent<TMP_InputField>().text = "joints_renamed";
            detailMenuNameInput.GetComponent<TMP_InputField>().SendMessage("SendOnEndEdit");
            yield return new WaitForSeconds(apiWaitingTime);
            ProjectRobotJoints joints;
            try {
                joints = actionPoint.GetJointsByName("joints_renamed");
            } catch (KeyNotFoundException ex) {
                joints = null;
            }
            yield return StartCoroutine(Q.assert.IsTrue(!joints.IsNull(), "Orientation name should be changed to \"joints_renamed\"."));


            //delete
            yield return StartCoroutine(Q.driver.Click(detailMenuDeleteBtn, "Click on delete button"));
            yield return StartCoroutine(Q.driver.Click(confirmationDialogOKButton, "Click on ok button."));
            yield return new WaitForSeconds(apiWaitingTime);
            yield return StartCoroutine(Q.assert.IsTrue(MenuManager.Instance.OrientationJointsDetailMenu.CurrentState == SimpleSideMenu.State.Closed, "Joints detail menu should be closed"));
            yield return StartCoroutine(Q.assert.IsTrue(jointsDynamicList.transform.childCount == --currentNumberOfJoints, "There should be " + currentNumberOfJoints + " joints in list"));


            //close aiming menu
            yield return StartCoroutine(Q.driver.Click(backButton, "Click on back button"));
            yield return StartCoroutine(Q.assert.IsTrue(MenuManager.Instance.ActionPointAimingMenu.CurrentState == SimpleSideMenu.State.Closed, "Aiming menu should be closed"));
        }

        [DependencyTest(9)]
        [Automation("Action point aiming menu tests")]
        public IEnumerator JointsDetailMenuExpertModeTest() {
            GameManager.Instance.ExpertMode = true;

            //open aiming menu
            yield return StartCoroutine(Q.driver.Click(focusButton, "Click on aiming menu icon."));
            yield return StartCoroutine(Q.assert.IsTrue(MenuManager.Instance.ActionPointAimingMenu.CurrentState == SimpleSideMenu.State.Open, "Aiming menu should be opened"));


            //open joints
            ActionButton jointsBtn = jointsDynamicList.GetComponentsInChildren<ActionButton>()[0]; //there should be joints from addJointsTest
            yield return StartCoroutine(Q.driver.Click(jointsBtn, "Click on joints."));
            yield return StartCoroutine(Q.assert.IsTrue(MenuManager.Instance.OrientationJointsDetailMenu.CurrentState == SimpleSideMenu.State.Open, "Joints detail menu should be opened"));


            yield return StartCoroutine(Q.assert.IsTrue(!detailMenuOrientationBlock.gameObject.activeSelf, "Orientation block should not be active"));
            yield return StartCoroutine(Q.assert.IsTrue(!detailMenuOrientationExpertBlock.gameObject.activeSelf, "Orientation expert block should not be active"));
            yield return StartCoroutine(Q.assert.IsTrue(detailMenuJointsBlock.gameObject.activeSelf, "Joints block should be active"));
            yield return StartCoroutine(Q.assert.IsTrue(detailMenuJointsExpertBlock.gameObject.activeSelf, "Joints expert block should be active"));
            yield return StartCoroutine(Q.assert.IsTrue(detailMenuMoveHereBlock.gameObject.activeSelf, "Move here block should be active"));
            yield return StartCoroutine(Q.assert.IsTrue(detailMenuUpdateBtn.GetComponent<Button>().interactable, "Update button should be interactable"));


            //set joints manually test
            yield return StartCoroutine(Q.driver.Click(detailMenuEditJointsCollapsable, "Click on edit joints collapsable."));
            var firstJointInput = detailMenuJointsDynamicList.GetComponentInChildren<LabeledInput>();
            firstJointInput.SetValue("3");
            yield return StartCoroutine(Q.driver.Click(detailMenuJointsManualSaveBtn, "Click on save joints."));
            yield return new WaitForSeconds(apiWaitingTime);
            var joints = actionPoint.GetJointsByName(detailMenuNameInput.GetComponent<TMP_InputField>().text);
            yield return StartCoroutine(Q.assert.IsTrue(joints.Joints[0].Value == 3m, "Joints didnt update (first joint should have value 3"));


            //update test
            yield return StartCoroutine(Q.driver.Click(detailMenuUpdateBtn, "Click on update button."));
            yield return StartCoroutine(Q.driver.Click(confirmationDialogOKButton, "Click on ok button."));
            yield return new WaitForSeconds(apiWaitingTime);
            yield return StartCoroutine(Q.assert.IsTrue(joints.Joints[0].Value != 3m, "Joints didnt update (value of first joint is still 3)"));


            //delete
            yield return StartCoroutine(Q.driver.Click(detailMenuDeleteBtn, "Click on delete button"));
            yield return StartCoroutine(Q.driver.Click(confirmationDialogOKButton, "Click on ok button."));
            yield return new WaitForSeconds(apiWaitingTime);
            yield return StartCoroutine(Q.assert.IsTrue(MenuManager.Instance.OrientationJointsDetailMenu.CurrentState == SimpleSideMenu.State.Closed, "Joints detail menu should be closed"));
            yield return StartCoroutine(Q.assert.IsTrue(jointsDynamicList.transform.childCount == --currentNumberOfJoints, "There should be " + currentNumberOfJoints + " joints in list"));


            //close aiming menu
            yield return StartCoroutine(Q.driver.Click(backButton, "Click on back button"));
            yield return StartCoroutine(Q.assert.IsTrue(MenuManager.Instance.ActionPointAimingMenu.CurrentState == SimpleSideMenu.State.Closed, "Aiming menu should be closed"));
        }


        // ---------------------------------------------------------------------
        //Following tests with project without a robot
        // ---------------------------------------------------------------------


        [DependencyTest(10)]
        [Automation("Action point aiming menu tests")]
        public IEnumerator AimingMenuNoRobotExpertModeTest() {
            //WebsocketManager.Instance.StopScene(false);
            yield return new WaitForSeconds(apiWaitingTime);
            GameManager.Instance.CloseProject(true);
            yield return new WaitForSeconds(4); //wait for project to close


            foreach (IO.Swagger.Model.ListProjectsResponseData project in GameManager.Instance.Projects) {
                if (project.Name == NameOfProjectWithoutRobot) {
                    GameManager.Instance.OpenProject(project.Id);
                    break;
                }
            }

            yield return new WaitForSeconds(4); //wait for project to load
            WebsocketManager.Instance.StartScene(false);
            yield return new WaitForSeconds(apiWaitingTime);
            GameManager.Instance.ExpertMode = true;
            currentNumberOfOrientations = 1; //reset counter for the new project

            //open action point menu
            actionPoint = ProjectManager.Instance.GetactionpointByName("sph_ap");
            actionPoint.OnClick(Clickable.Click.MOUSE_RIGHT_BUTTON);

            //open aiming menu
            yield return StartCoroutine(Q.driver.Click(focusButton, "Click on aiming menu icon."));
            yield return StartCoroutine(Q.assert.IsTrue(MenuManager.Instance.ActionPointAimingMenu.CurrentState == SimpleSideMenu.State.Open, "Aiming menu should be opened"));

            yield return StartCoroutine(Q.assert.IsTrue(positionCollapsable.gameObject.activeSelf, "Position collapsable menu should be active."));
            yield return StartCoroutine(Q.assert.IsTrue(orientationCollapsable.gameObject.activeSelf, "Orientations collapsable menu should be active."));
            yield return StartCoroutine(Q.assert.IsTrue(jointsCollapsable.gameObject.activeSelf, "Joints collapsable menu should be active."));


            //position collapsable menu tests
            yield return StartCoroutine(Q.assert.IsTrue(!positionUpdateUsingRobotBtn.GetComponent<Button>().interactable, "Position update using robot button should not be interactable"));
            yield return StartCoroutine(Q.assert.IsTrue(positionExpertBlock.gameObject.activeSelf, "Update position manually block should be active"));
            yield return StartCoroutine(Q.assert.IsTrue(positionManualEdit.gameObject.activeSelf, "Position manual editing should be active"));

            Position apPosition = positionManualEdit.GetComponent<PositionManualEdit>().GetPosition();
            yield return StartCoroutine(Q.assert.IsTrue(apPosition.X == 0m && apPosition.Y == 0m && apPosition.Z == 0.2m, "Position values should be 0;0;0.2"));
            //edit position manually
            positionX.GetComponent<TMP_InputField>().text = "0.3";
            positionY.GetComponent<TMP_InputField>().text = "-0.2";
            positionZ.GetComponent<TMP_InputField>().text = "0.15";
            yield return StartCoroutine(Q.driver.Click(positionUpdateManualBtn, "Click on update position manually."));
            yield return new WaitForSeconds(apiWaitingTime);
            var newPosition = actionPoint.Data.Position;
            yield return StartCoroutine(Q.assert.IsTrue(newPosition.X == 0.3m && newPosition.Y == -0.2m && newPosition.Z == 0.15m, "Position of action point is not set right"));


            //orientation collapsable menu tests
            yield return StartCoroutine(Q.assert.IsTrue(!orientationAddUsingRobotBtn.GetComponent<Button>().interactable, "Add orientation using robot button should not be interactable"));
            yield return StartCoroutine(Q.assert.IsTrue(orientationAddManualDefaultBtn.gameObject.activeSelf, "Add orientation manually button should be active"));
            yield return StartCoroutine(Q.assert.IsTrue(orientationAddManualDefaultBtn.GetComponent<ActionButton>().GetLabel() == "Manual", "Label of right button in orientation add section should be 'Manual'"));
            yield return StartCoroutine(Q.assert.IsTrue(orientationsDynamicList.transform.childCount == currentNumberOfOrientations, "There should be one orientation in orientations list"));


            //close aiming menu
            yield return StartCoroutine(Q.driver.Click(backButton, "Click on back button"));
            yield return StartCoroutine(Q.assert.IsTrue(MenuManager.Instance.ActionPointAimingMenu.CurrentState == SimpleSideMenu.State.Closed, "Aiming menu should be closed"));
        }


        [DependencyTest(11)]
        [Automation("Action point aiming menu tests")]
        public IEnumerator AimingMenuNoRobotLiteModeTest() {
            GameManager.Instance.ExpertMode = false;

            //open aiming menu
            yield return StartCoroutine(Q.driver.Click(focusButton, "Click on aiming menu icon."));
            yield return StartCoroutine(Q.assert.IsTrue(MenuManager.Instance.ActionPointAimingMenu.CurrentState == SimpleSideMenu.State.Open, "Aiming menu should be opened"));

            yield return StartCoroutine(Q.assert.IsTrue(positionCollapsable.gameObject.activeSelf, "Position collapsable menu should be active."));
            yield return StartCoroutine(Q.assert.IsTrue(orientationCollapsable.gameObject.activeSelf, "Orientations collapsable menu should be active."));
            yield return StartCoroutine(Q.assert.IsTrue(jointsCollapsable.gameObject.activeSelf, "Joints collapsable menu should be active."));


            //orientation collapsable menu tests
            yield return StartCoroutine(Q.assert.IsTrue(!orientationAddUsingRobotBtn.GetComponent<Button>().interactable, "Add orientation using robot button should not be interactable"));
            yield return StartCoroutine(Q.assert.IsTrue(orientationAddManualDefaultBtn.gameObject.activeSelf, "Add default orientation button should be active"));
            yield return StartCoroutine(Q.assert.IsTrue(orientationAddManualDefaultBtn.GetComponent<ActionButton>().GetLabel() == "Default", "Label of right button in orientation add section should be 'Default'"));
            yield return StartCoroutine(Q.assert.IsTrue(orientationsDynamicList.transform.childCount == currentNumberOfOrientations, "There should be one orientation in orientations list"));


            //close aiming menu
            yield return StartCoroutine(Q.driver.Click(backButton, "Click on back button"));
            yield return StartCoroutine(Q.assert.IsTrue(MenuManager.Instance.ActionPointAimingMenu.CurrentState == SimpleSideMenu.State.Closed, "Aiming menu should be closed"));
        }


        [DependencyTest(12)]
        [Automation("Action point aiming menu tests")]
        public IEnumerator AddOrientationNoRobotLiteModeTest() {
            GameManager.Instance.ExpertMode = false;

            //open aiming menu
            yield return StartCoroutine(Q.driver.Click(focusButton, "Click on aiming menu icon."));
            yield return StartCoroutine(Q.assert.IsTrue(MenuManager.Instance.ActionPointAimingMenu.CurrentState == SimpleSideMenu.State.Open, "Aiming menu should be opened"));


            //add default
            yield return StartCoroutine(Q.driver.Click(orientationAddManualDefaultBtn, "Click on add default orientation."));
            yield return new WaitForSeconds(apiWaitingTime);
            yield return StartCoroutine(Q.assert.IsTrue(orientationsDynamicList.transform.childCount == ++currentNumberOfOrientations, "There should be " + currentNumberOfOrientations + " orientations in list now"));


            //close aiming menu
            yield return StartCoroutine(Q.driver.Click(backButton, "Click on back button"));
            yield return StartCoroutine(Q.assert.IsTrue(MenuManager.Instance.ActionPointAimingMenu.CurrentState == SimpleSideMenu.State.Closed, "Aiming menu should be closed"));
        }


        [DependencyTest(13)]
        [Automation("Action point aiming menu tests")]
        public IEnumerator AddOrientationNoRobotExpertModeTest() {
            GameManager.Instance.ExpertMode = true;

            //open aiming menu
            yield return StartCoroutine(Q.driver.Click(focusButton, "Click on aiming menu icon."));
            yield return StartCoroutine(Q.assert.IsTrue(MenuManager.Instance.ActionPointAimingMenu.CurrentState == SimpleSideMenu.State.Open, "Aiming menu should be opened"));


            //add manual
            yield return StartCoroutine(Q.driver.Click(orientationAddManualDefaultBtn, "Click on add manual orientation."));
            yield return StartCoroutine(Q.assert.IsTrue(MenuManager.Instance.AddOrientationMenu.CurrentState == SimpleSideMenu.State.Open, "Add orientation menu should be opened"));
            yield return StartCoroutine(Q.assert.IsTrue(addOrientationMenuExpertBlock.gameObject.activeSelf, "Expert mode block should  be active"));
            yield return StartCoroutine(Q.assert.IsTrue(!addOrientationMenuRobotPickBlock.gameObject.activeSelf, "Robot selection block should not be active"));
            string orientationName = "ori_manual";
            addOrientationMenuNameInput.GetComponent<TMP_InputField>().text = orientationName;

            yield return StartCoroutine(Q.driver.Click(addOrientationMenuQuaternionEulerSwitch, "Click on switch between quaternion and euler angles."));
            yield return StartCoroutine(Q.assert.IsTrue(!addOrientationMenuWInput.gameObject.activeSelf, "W input should not be active"));
            yield return StartCoroutine(Q.driver.Click(addOrientationMenuQuaternionEulerSwitch, "Click on switch between quaternion and euler angles."));
            yield return StartCoroutine(Q.assert.IsTrue(addOrientationMenuWInput.gameObject.activeSelf, "W input should be active"));

            addOrientationMenuXInput.GetComponent<TMP_InputField>().text = "0.5";
            addOrientationMenuWInput.GetComponent<TMP_InputField>().text = "1";
            yield return StartCoroutine(Q.driver.Click(addOrientationMenuCreateBtn, "Click on ok button (create orientation using robot)."));
            yield return new WaitForSeconds(apiWaitingTime);
            yield return StartCoroutine(Q.assert.IsTrue(MenuManager.Instance.AddOrientationMenu.CurrentState == SimpleSideMenu.State.Closed, "Add orientation menu should be closed"));
            yield return StartCoroutine(Q.assert.IsTrue(orientationsDynamicList.transform.childCount == ++currentNumberOfOrientations, "There should be " + currentNumberOfOrientations + " orientations in list now"));
            NamedOrientation orientation;
            try {
                orientation = actionPoint.GetOrientationByName(orientationName);
            } catch (KeyNotFoundException ex) {
                orientation = null;
            }
            yield return StartCoroutine(Q.assert.IsTrue(!orientation.IsNull(), "Orientation not found by name!"));
            yield return StartCoroutine(Q.assert.IsTrue(orientation.Orientation.X - 0.4472m < 0.001m, "Orientation saved with wrong value"));
            yield return StartCoroutine(Q.assert.IsTrue(orientation.Orientation.W - 0.8944m < 0.001m, "Orientation saved with wrong value"));


            //close aiming menu
            yield return StartCoroutine(Q.driver.Click(backButton, "Click on back button"));
            yield return StartCoroutine(Q.assert.IsTrue(MenuManager.Instance.ActionPointAimingMenu.CurrentState == SimpleSideMenu.State.Closed, "Aiming menu should be closed"));
        }


        [DependencyTest(14)]
        [Automation("Action point aiming menu tests")]
        public IEnumerator OrientationDetailNoRobotMenuExpertModeTest() {
            GameManager.Instance.ExpertMode = true;

            //open aiming menu
            yield return StartCoroutine(Q.driver.Click(focusButton, "Click on aiming menu icon."));
            yield return StartCoroutine(Q.assert.IsTrue(MenuManager.Instance.ActionPointAimingMenu.CurrentState == SimpleSideMenu.State.Open, "Aiming menu should be opened"));


            //open "default" orientation
            ActionButton orientationBtn = null;
            ActionButton[] orientationsBtns = orientationsDynamicList.GetComponentsInChildren<ActionButton>();
            foreach (ActionButton btn in orientationsBtns) {
                if (btn.GetLabel() == "default") {
                    orientationBtn = btn;
                    break;
                }
            }
            yield return StartCoroutine(Q.driver.Click(orientationBtn, "Click on default orientation."));
            yield return StartCoroutine(Q.assert.IsTrue(MenuManager.Instance.OrientationJointsDetailMenu.CurrentState == SimpleSideMenu.State.Open, "Orientation detail menu should be opened"));


            yield return StartCoroutine(Q.assert.IsTrue(detailMenuNameInput.GetComponent<TMP_InputField>().text == "default", "Orientation name should be \"default\""));
            yield return StartCoroutine(Q.assert.IsTrue(!detailMenuOrientationBlock.gameObject.activeSelf, "Orientation block should not be active"));
            yield return StartCoroutine(Q.assert.IsTrue(detailMenuOrientationExpertBlock.gameObject.activeSelf, "Orientation expert block should be active"));
            yield return StartCoroutine(Q.assert.IsTrue(!detailMenuJointsBlock.gameObject.activeSelf, "Joints block should not be active"));
            yield return StartCoroutine(Q.assert.IsTrue(!detailMenuJointsExpertBlock.gameObject.activeSelf, "Joints expert block should not be active"));
            yield return StartCoroutine(Q.assert.IsTrue(!detailMenuMoveHereBlock.gameObject.activeSelf, "Move here block should not be active"));
            yield return StartCoroutine(Q.assert.IsTrue(!detailMenuUpdateBtn.GetComponent<Button>().interactable, "Update button should be not interactable"));
            yield return StartCoroutine(Q.assert.IsTrue(detailMenuOrientationManualEdit.GetComponent<OrientationManualEdit>().GetOrientation().Equals(new Orientation()), "Orientation should be 0,0,0,1"));


            //edit orientation manually (euler angles)
            yield return StartCoroutine(Q.driver.Click(detailMenuEditOrientationCollapsable, "Click on edit orientation collapsable."));
            yield return StartCoroutine(Q.driver.Click(detailMenuOrientationQuaternionEulerSwitch, "Click on switch to euler angles."));
            detailMenuOrientationXInput.GetComponent<TMP_InputField>().text = "50";
            yield return StartCoroutine(Q.driver.Click(detailMenuOrientationManualSaveBtn, "Click on save euler angles."));
            yield return new WaitForSeconds(apiWaitingTime);
            NamedOrientation orientation = actionPoint.GetNamedOrientationByName("default");
            yield return StartCoroutine(Q.assert.IsTrue(orientation.Orientation.X - 0.4226m < 0.001m, "Orientation saved with wrong value"));
            yield return StartCoroutine(Q.assert.IsTrue(orientation.Orientation.W - 0.9063m < 0.001m, "Orientation saved with wrong value"));


            //close
            yield return StartCoroutine(Q.driver.Click(detailMenuBackBtn, "Click on back button (close detail menu)"));
            yield return StartCoroutine(Q.assert.IsTrue(MenuManager.Instance.OrientationJointsDetailMenu.CurrentState == SimpleSideMenu.State.Closed, "Orientation detail menu should be closed"));

            //close aiming menu
            yield return StartCoroutine(Q.driver.Click(backButton, "Click on back button"));
            yield return StartCoroutine(Q.assert.IsTrue(MenuManager.Instance.ActionPointAimingMenu.CurrentState == SimpleSideMenu.State.Closed, "Aiming menu should be closed"));
        }

        [DependencyTest(15)]
        [Automation("Action point aiming menu tests")]
        public IEnumerator OrientationDetailMenuNoRobotLiteModeTest() {
            GameManager.Instance.ExpertMode = false;

            //open aiming menu
            yield return StartCoroutine(Q.driver.Click(focusButton, "Click on aiming menu icon."));
            yield return StartCoroutine(Q.assert.IsTrue(MenuManager.Instance.ActionPointAimingMenu.CurrentState == SimpleSideMenu.State.Open, "Aiming menu should be opened"));


            //open "default" orientation
            ActionButton orientationBtn = null;
            ActionButton[] orientationsBtns = orientationsDynamicList.GetComponentsInChildren<ActionButton>();
            foreach (ActionButton btn in orientationsBtns) {
                if (btn.GetLabel() == "default") {
                    orientationBtn = btn;
                    break;
                }
            }
            yield return StartCoroutine(Q.driver.Click(orientationBtn, "Click on default orientation."));
            yield return StartCoroutine(Q.assert.IsTrue(MenuManager.Instance.OrientationJointsDetailMenu.CurrentState == SimpleSideMenu.State.Open, "Orientation detail menu should be opened"));


            yield return StartCoroutine(Q.assert.IsTrue(detailMenuNameInput.GetComponent<TMP_InputField>().text == "default", "Orientation name should be \"default\""));
            yield return StartCoroutine(Q.assert.IsTrue(!detailMenuOrientationBlock.gameObject.activeSelf, "Orientation block should not be active"));
            yield return StartCoroutine(Q.assert.IsTrue(!detailMenuOrientationExpertBlock.gameObject.activeSelf, "Orientation expert block should not be active"));
            yield return StartCoroutine(Q.assert.IsTrue(!detailMenuJointsBlock.gameObject.activeSelf, "Joints block should not be active"));
            yield return StartCoroutine(Q.assert.IsTrue(!detailMenuJointsExpertBlock.gameObject.activeSelf, "Joints expert block should not be active"));
            yield return StartCoroutine(Q.assert.IsTrue(!detailMenuMoveHereBlock.gameObject.activeSelf, "Move here block should not be active"));
            yield return StartCoroutine(Q.assert.IsTrue(!detailMenuUpdateBtn.GetComponent<Button>().interactable, "Update button should not be interactable"));


            //rename test
            detailMenuNameInput.GetComponent<TMP_InputField>().text = "new_name";
            detailMenuNameInput.GetComponent<TMP_InputField>().SendMessage("SendOnEndEdit");
            yield return new WaitForSeconds(apiWaitingTime);
            NamedOrientation orientation;
            try {
                orientation = actionPoint.GetOrientationByName("new_name");
            } catch (KeyNotFoundException ex) {
                orientation = null;
            }
            yield return StartCoroutine(Q.assert.IsTrue(!orientation.IsNull(), "Orientation name should be changed to \"new_name\"."));


            //delete
            yield return StartCoroutine(Q.driver.Click(detailMenuDeleteBtn, "Click on delete button"));
            yield return StartCoroutine(Q.driver.Click(confirmationDialogOKButton, "Click on ok button."));
            yield return new WaitForSeconds(apiWaitingTime);
            yield return StartCoroutine(Q.assert.IsTrue(MenuManager.Instance.OrientationJointsDetailMenu.CurrentState == SimpleSideMenu.State.Closed, "Orientation detail menu should be closed"));
            yield return StartCoroutine(Q.assert.IsTrue(orientationsDynamicList.transform.childCount == --currentNumberOfOrientations, "There should be " + currentNumberOfOrientations + " orientation in orientations list"));


            //close aiming menu
            yield return StartCoroutine(Q.driver.Click(backButton, "Click on back button"));
            yield return StartCoroutine(Q.assert.IsTrue(MenuManager.Instance.ActionPointAimingMenu.CurrentState == SimpleSideMenu.State.Closed, "Aiming menu should be closed"));
        }

        [TearDown]
        public IEnumerator TearDown()
        {

            yield return null;

        }

        [TearDownClass]
        public IEnumerator TearDownClass()
        {
            GameManager.Instance.CloseProject(true);

            yield return null;

        }

    }*/

}
