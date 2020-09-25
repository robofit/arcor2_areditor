using UnityEngine;
using System.Collections;
using System.Diagnostics.Tracing;
using Base;

namespace TrilleonAutomation
{

    [AutomationClass]
    public class LoginTests : MonoBehaviour
    {
        GameObject connectBtn;
        GameObject disconnectBtn;
        GameObject keepMeConnectedToggle;
       [SetUpClass]
        public IEnumerator SetUpClass()
        {

            yield return null;

        }

        [SetUp]
        public IEnumerator SetUp()
        {
            connectBtn = Q.driver.Find(By.Name, "ConnectBtn");
            disconnectBtn = Q.driver.Find(By.Name, "DisconnectBtn");
            keepMeConnectedToggle = Q.driver.Find(By.Name, "KeepMeConnectedToggle");
            yield return null;

        }

        [Automation("Login tests")]
        [DependencyTest(1)]
        public IEnumerator UserCanSetKeepMeConnectedTest() {
            yield return StartCoroutine(Q.driver.Click(keepMeConnectedToggle, "Click keep me connected toggle."));
            yield return StartCoroutine(Q.assert.IsTrue(PlayerPrefsHelper.LoadBool("arserver_keep_connected", false), "Should be true"));
            yield return StartCoroutine(Q.driver.Click(keepMeConnectedToggle, "Click keep me connected toggle."));
            yield return StartCoroutine(Q.assert.IsTrue(!PlayerPrefsHelper.LoadBool("arserver_keep_connected", true), "Should be false"));

        }

        [DependencyTest(2)]
        [Automation("Login tests")]
        public IEnumerator UserCanConnectToServerTest()
        {
            // yield return StartCoroutine(Q.assert.IsTrue(true, "This will pass."));
            //' yield return StartCoroutine(Q.assert.IsTrue(false, "This will fail."));' yield return null;
            yield return StartCoroutine(Q.driver.Click(connectBtn, "Click connect button."));
            yield return StartCoroutine(Q.driver.WaitFor(() => GameManager.Instance.GetGameState() != GameManager.GameStateEnum.Disconnected));
            yield return StartCoroutine(Q.assert.IsTrue(MainScreen.Instance.IsActive(), "Main screen should be active after first login"));
            yield return StartCoroutine(Q.assert.IsTrue(LandingScreen.Instance.IsInactive(), "Landing screen should be inactive after first login"));
        }

        [DependencyTest(3)]
        [Automation("Login tests")]
        public IEnumerator UserCanDisconnectFromServerTest() {
            yield return StartCoroutine(Q.driver.Click(disconnectBtn, "Click disconnect button."));
            yield return StartCoroutine(Q.driver.WaitFor(() => GameManager.Instance.GetGameState() == GameManager.GameStateEnum.Disconnected));
            yield return StartCoroutine(Q.assert.IsTrue(MainScreen.Instance.IsInactive(), "Main screen should be inactive after first login"));
            yield return StartCoroutine(Q.assert.IsTrue(LandingScreen.Instance.IsActive(), "Landing screen should be active after first login"));
        }

        [DependencyTest(4)]
        [Automation("Login tests")]
        public IEnumerator UserCanReconnectFromServerTest() {
            yield return UserCanConnectToServerTest();
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
