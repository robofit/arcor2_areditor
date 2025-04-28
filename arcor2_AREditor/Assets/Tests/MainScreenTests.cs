using System.Collections;
using Arcor2.ClientSdk.Communication.OpenApi.Models;
using Base;
using DanielLochner.Assets.SimpleSideMenu;
using UnityEngine;

namespace TrilleonAutomation
{

    [AutomationClass]
    public class MainScreenTests : MonoBehaviour
    {
        GameObject sceneseBtn, projectsBtn, packagesBtn;
        GameObject newSceneProjectBtn, saveSceneBtn, closeSceneBtn;
        GameObject newProjectBtn, saveProjectBtn, closeProjectBtn;
        GameObject inputDialog, confirmationDialog;
        GameObject inputDialogInput;
        GameObject inputDialogOKButton;
        GameObject confirmationDialogOKButton;
        GameObject newProjectDialogOKButton;
        SceneOptionMenu sceneOptionMenu;
        ProjectOptionMenu projectOptionMenu;
        NewProjectDialog newProjectDialog;
        GameObject sceneRename, sceneRemove;
        GameObject projectRename, projectRemove;
        [SetUpClass]
        public IEnumerator SetUpClass()
        {

            yield return null;

        }

        [SetUp]
        public IEnumerator SetUp()
        {
            GameObject buttonsLandscape = Q.driver.Find(By.Name, "ButtonsLandscape");
            GameObject scenesList = Q.driver.Find(By.Name, "ScenesList");
            GameObject projectsList = Q.driver.Find(By.Name, "ProjectsList");
            GameObject mainScreen = Q.driver.Find(By.Name, "MainScreen");

            sceneseBtn = Q.driver.FindIn(buttonsLandscape, By.Name, "ScenesButton");
            projectsBtn = Q.driver.FindIn(buttonsLandscape, By.Name, "ProjectsButton");
            packagesBtn = Q.driver.FindIn(buttonsLandscape, By.Name, "PackagesButton");

            saveSceneBtn = Q.driver.Find(By.Name, "SaveScene");
            closeSceneBtn = Q.driver.Find(By.Name, "CloseScene");

            saveProjectBtn = Q.driver.Find(By.Name, "SaveProject");
            closeProjectBtn = Q.driver.Find(By.Name, "CloseProject");

            newSceneProjectBtn = Q.driver.Find(By.Name, "AddNewSceneProject");
            Debug.Assert(newSceneProjectBtn != null, "newSceneProjectBtn should not be null");

            inputDialog = Q.driver.FindIn(mainScreen, By.Name, "InputDialog");

            inputDialogInput = Q.driver.FindIn(inputDialog, By.Name, "Input");
            inputDialogOKButton = Q.driver.FindIn(inputDialog, By.Name, "Got It");

            sceneOptionMenu = MainScreen.Instance.SceneOptionMenu;
            sceneRename = Q.driver.FindIn(sceneOptionMenu.gameObject, By.Name, "Rename");
            sceneRemove = Q.driver.FindIn(sceneOptionMenu.gameObject, By.Name, "Delete");
            projectOptionMenu = MainScreen.Instance.ProjectOptionMenu;
            projectRename = Q.driver.FindIn(projectOptionMenu.gameObject, By.Name, "Rename");
            projectRemove = Q.driver.FindIn(projectOptionMenu.gameObject, By.Name, "Delete");

            confirmationDialog = sceneOptionMenu.ConfirmationDialog.gameObject;
            confirmationDialogOKButton = Q.driver.FindIn(confirmationDialog, By.Name, "Ok");

            newProjectDialog = Q.driver.Find(By.Name, "NewProjectDialog").GetComponent<NewProjectDialog>();
            newProjectDialogOKButton = Q.driver.FindIn(newProjectDialog.gameObject, By.Name, "Got It");
            yield return null;
        }
        [DependencyTest(1)]
        [Automation("Main screen tests")]
        public IEnumerator UserCanSwitchListsTest()
        {
            yield return StartCoroutine(Q.assert.IsTrue(MainScreen.Instance.ScenesList.gameObject.activeSelf, "Scenes should be active"));
            yield return StartCoroutine(Q.assert.IsTrue(!MainScreen.Instance.ProjectsList.gameObject.activeSelf, "Projects should be inactive"));
            yield return StartCoroutine(Q.assert.IsTrue(!MainScreen.Instance.PackageList.gameObject.activeSelf, "Packages should be inactive"));
            yield return StartCoroutine(Q.driver.Click(projectsBtn, "Click on projects."));
            yield return StartCoroutine(Q.assert.IsTrue(!MainScreen.Instance.ScenesList.gameObject.activeSelf, "Scenes should be inactive"));
            yield return StartCoroutine(Q.assert.IsTrue(MainScreen.Instance.ProjectsList.gameObject.activeSelf, "Projects should be active"));
            yield return StartCoroutine(Q.assert.IsTrue(!MainScreen.Instance.PackageList.gameObject.activeSelf, "Packages should be inactive"));
            yield return StartCoroutine(Q.driver.Click(packagesBtn, "Click on packages."));
            yield return StartCoroutine(Q.assert.IsTrue(!MainScreen.Instance.ScenesList.gameObject.activeSelf, "Scenes should be inactive"));
            yield return StartCoroutine(Q.assert.IsTrue(!MainScreen.Instance.ProjectsList.gameObject.activeSelf, "Projects should be inactive"));
            yield return StartCoroutine(Q.assert.IsTrue(MainScreen.Instance.PackageList.gameObject.activeSelf, "Packages should be active"));
            yield return StartCoroutine(Q.driver.Click(sceneseBtn, "Click on scenes."));
            yield return StartCoroutine(Q.assert.IsTrue(MainScreen.Instance.ScenesList.gameObject.activeSelf, "Scenes should be active"));
            yield return StartCoroutine(Q.assert.IsTrue(!MainScreen.Instance.ProjectsList.gameObject.activeSelf, "Projects should be inactive"));
            yield return StartCoroutine(Q.assert.IsTrue(!MainScreen.Instance.PackageList.gameObject.activeSelf, "Packages should be inactive"));

        }
        [DependencyTest(2)]
        [Automation("Main screen tests")]
        public IEnumerator UserCanCreateAndSaveSceneTest() {
            yield return StartCoroutine(Q.driver.Click(newSceneProjectBtn, "Click on new scene button"));
            inputDialog.GetComponent<InputDialog>().SetInputValue("test scene");
            yield return StartCoroutine(Q.driver.Click(inputDialogOKButton, "Click on Got it button"));

            yield return StartCoroutine(Q.driver.Click(saveSceneBtn, "Click on save scene button"));
            yield return StartCoroutine(Q.driver.Click(closeSceneBtn, "Click on close scene button"));
            bool sceneExist = false;
            foreach (ListScenesResponseData scene in GameManager.Instance.Scenes) {
                if (scene.Name == "test scene")
                    sceneExist = true;
            }
            yield return StartCoroutine(Q.assert.IsTrue(sceneExist, "Test scene does not exist"));
            
        }

        [DependencyTest(3)]
        [Automation("Main screen tests")]
        public  IEnumerator UserCanCreateAndSaveProjectTest() {
            yield return StartCoroutine(Q.driver.Click(projectsBtn, "Click on projects."));
            yield return StartCoroutine(Q.driver.Click(newSceneProjectBtn, "Click on new project button"));
            newProjectDialog.NewProjectName.SetValue("test project");
            yield return StartCoroutine(Q.driver.Click(newProjectDialogOKButton, "Click on Got it button"));
            // not working.. solve better
            _ = CommunicationManager.Instance.Client.AddActionPointAsync(new AddActionPointRequestArgs("asdf", new Position()));
            yield return new WaitForSeconds(2);
            yield return StartCoroutine(Q.driver.Click(saveProjectBtn, "Click on save project button"));
            yield return StartCoroutine(Q.driver.Click(closeProjectBtn, "Click on close project button"));
            bool projectExists = false;
            foreach (ListProjectsResponseData project in GameManager.Instance.Projects) {
                if (project.Name == "test project")
                    projectExists = true;
            }
            yield return StartCoroutine(Q.assert.IsTrue(projectExists, "Test project does not exist"));
        }

        [DependencyTest(4)]
        [Automation("Main screen tests")]
        public IEnumerator UserCanRenameSceneTest() {
            yield return StartCoroutine(Q.driver.Click(sceneseBtn, "Click on scenes."));
            SceneTile sceneTile = MainScreen.Instance.GetSceneTile("test scene");
            yield return StartCoroutine(Q.assert.IsTrue(sceneTile != null, "Scene not found!"));
            yield return StartCoroutine(Q.driver.Click(sceneTile.GetOptionButton().gameObject, "Click on option menu."));
            yield return StartCoroutine(Q.assert.IsTrue(sceneOptionMenu.GetComponent<SimpleSideMenu>().CurrentState == SimpleSideMenu.State.Open, "Option menu not opened!"));
            yield return StartCoroutine(Q.driver.Click(sceneRename, "Click on scene rename."));
            inputDialog.GetComponent<InputDialog>().SetInputValue("test scene renamed");
            yield return StartCoroutine(Q.driver.Click(inputDialogOKButton, "Click on Got it button"));
            // wait for server to respond
            yield return new WaitForSeconds(2);
            sceneTile = MainScreen.Instance.GetSceneTile("test scene renamed");
            yield return StartCoroutine(Q.assert.IsTrue(sceneTile != null, "Scene not found!"));
            bool sceneExist = false;
            foreach (ListScenesResponseData scene in GameManager.Instance.Scenes) {
                if (scene.Name == "test scene renamed")
                    sceneExist = true;
            }
            yield return StartCoroutine(Q.assert.IsTrue(sceneExist, "Test scene does not exist"));
        }

        // cant remove when there is project.. so check that it cannot be removed instead, then remove project, then remove scene
        [DependencyTest(5)]
        [Automation("Main screen tests")]
        public IEnumerator UserCannotRemoveSceneWhenThereIsProjectTest() {
            SceneTile sceneTile = MainScreen.Instance.GetSceneTile("test scene renamed");
            yield return StartCoroutine(Q.assert.IsTrue(sceneTile != null, "Scene not found!"));
            yield return StartCoroutine(Q.driver.Click(sceneTile.GetOptionButton().gameObject, "Click on option menu."));
            yield return StartCoroutine(Q.assert.IsTrue(sceneOptionMenu.GetComponent<SimpleSideMenu>().CurrentState == SimpleSideMenu.State.Open, "Option menu not opened!"));
            yield return StartCoroutine(Q.driver.Click(sceneRemove, "Click on scene remove."));
            yield return StartCoroutine(Q.driver.Click(confirmationDialogOKButton, "Click on ok button"));
            // wait for server to respond
            yield return new WaitForSeconds(2);
            try {
                sceneTile = MainScreen.Instance.GetSceneTile("test scene renamed");
            } catch (ItemNotFoundException) {
                sceneTile = null;
            }            
            yield return StartCoroutine(Q.assert.IsTrue(sceneTile != null, "Scene should exists!"));
            bool sceneExist = false;
            foreach (ListScenesResponseData scene in GameManager.Instance.Scenes) {
                if (scene.Name == "test scene renamed")
                    sceneExist = true;
            }
            yield return StartCoroutine(Q.assert.IsTrue(sceneExist, "Test scene should exists"));
            sceneOptionMenu.GetComponent<SimpleSideMenu>().Close();
        }

        [DependencyTest(6)]
        [Automation("Main screen tests")]
        public IEnumerator UserCanRenameProjectTest() {
            yield return StartCoroutine(Q.driver.Click(projectsBtn, "Click on projects."));
            ProjectTile projectTile;
            try {
                projectTile = MainScreen.Instance.GetProjectTile("test project");
            } catch (ItemNotFoundException) {
                projectTile = null;
            }
            
            yield return StartCoroutine(Q.assert.IsTrue(projectTile != null, "Project not found!"));
            yield return StartCoroutine(Q.driver.Click(projectTile.GetOptionButton().gameObject, "Click on option menu."));
            yield return StartCoroutine(Q.assert.IsTrue(projectOptionMenu.GetComponent<SimpleSideMenu>().CurrentState == SimpleSideMenu.State.Open, "Option menu not opened!"));
            yield return StartCoroutine(Q.driver.Click(projectRename, "Click on scene rename."));
            inputDialog.GetComponent<InputDialog>().SetInputValue("test project renamed");
            yield return StartCoroutine(Q.driver.Click(inputDialogOKButton, "Click on Got it button"));
            // wait for server to respond
            yield return new WaitForSeconds(2);
            projectTile = MainScreen.Instance.GetProjectTile("test project renamed");
            yield return StartCoroutine(Q.assert.IsTrue(projectTile != null, "Project not found!"));
            bool projectExists = false;
            foreach (ListProjectsResponseData project in GameManager.Instance.Projects) {
                if (project.Name == "test project renamed")
                    projectExists = true;
            }
            yield return StartCoroutine(Q.assert.IsTrue(projectExists, "Test project does not exist"));
        }

        // cant remove when there is project.. so check that it cannot be removed instead, then remove project, then remove scene
        [DependencyTest(7)]
        [Automation("Main screen tests")]
        public IEnumerator UserCanRemoveProjectTest() {
            ProjectTile projectTile = MainScreen.Instance.GetProjectTile("test project renamed");
            yield return StartCoroutine(Q.assert.IsTrue(projectTile != null, "Project not found!"));
            yield return StartCoroutine(Q.driver.Click(projectTile.GetOptionButton().gameObject, "Click on option menu."));
            yield return StartCoroutine(Q.assert.IsTrue(projectOptionMenu.GetComponent<SimpleSideMenu>().CurrentState == SimpleSideMenu.State.Open, "Option menu not opened!"));
            yield return StartCoroutine(Q.driver.Click(projectRemove, "Click on scene remove."));
            yield return StartCoroutine(Q.driver.Click(confirmationDialogOKButton, "Click on ok button"));
            // wait for server to respond
            yield return new WaitForSeconds(2);
            try {
                projectTile = MainScreen.Instance.GetProjectTile("test project renamed");
            } catch (ItemNotFoundException) {
                projectTile = null;
            }
            yield return StartCoroutine(Q.assert.IsTrue(projectTile == null, "Project should be removed but it is not!"));
            bool projectExists = false;
            foreach (ListProjectsResponseData project in GameManager.Instance.Projects) {
                if (project.Name == "test scene renamed")
                    projectExists = true;
            }
            yield return StartCoroutine(Q.assert.IsTrue(!projectExists, "Test project should not exist"));
        }

        [DependencyTest(8)]
        [Automation("Main screen tests")]
        public IEnumerator UserCanRemoveSceneTest() {
            yield return StartCoroutine(Q.driver.Click(sceneseBtn, "Click on scenes."));
            SceneTile sceneTile = MainScreen.Instance.GetSceneTile("test scene renamed");
            yield return StartCoroutine(Q.assert.IsTrue(sceneTile != null, "Scene not found!"));
            yield return StartCoroutine(Q.driver.Click(sceneTile.GetOptionButton().gameObject, "Click on option menu."));
            yield return StartCoroutine(Q.assert.IsTrue(sceneOptionMenu.GetComponent<SimpleSideMenu>().CurrentState == SimpleSideMenu.State.Open, "Option menu not opened!"));
            yield return StartCoroutine(Q.driver.Click(sceneRemove, "Click on scene remove."));
            yield return StartCoroutine(Q.driver.Click(confirmationDialogOKButton, "Click on ok button"));
            // wait for server to respond
            yield return new WaitForSeconds(2);
            try {
                sceneTile = MainScreen.Instance.GetSceneTile("test scene renamed");
            } catch (ItemNotFoundException) {
                sceneTile = null;
            }
            yield return StartCoroutine(Q.assert.IsTrue(sceneTile == null, "Scene should be removed but it is not!"));
            bool sceneExist = false;
            foreach (ListScenesResponseData scene in GameManager.Instance.Scenes) {
                if (scene.Name == "test scene renamed")
                    sceneExist = true;
            }
            yield return StartCoroutine(Q.assert.IsTrue(!sceneExist, "Test scene should not exist"));
        }



        [TearDown]
        public IEnumerator TearDown()
        {

            yield return null;

        }

        [TearDownClass]
        public IEnumerator TearDownClass()
        {

            yield return null;

        }

    }

}
