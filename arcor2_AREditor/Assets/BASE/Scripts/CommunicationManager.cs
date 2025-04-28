using System;
using Arcor2.ClientSdk.Communication;
using UnityEngine;

/*
 * !!!!!!!!!!!!!!!
 * All events are unsafe for interaction with Mono objects, as they are raised from non-UI thread.
 * Wrap these in a SafeEventHandler<T>.
 * !!!!!!!!!!!!!!
 */

namespace Base {

    /// <summary>
    /// Manages communication with the remote ARCOR2 server.
    /// </summary>
    public class CommunicationManager : Singleton<CommunicationManager> {
        /// <summary>
        /// The ARCOR2 client used for communication.
        /// </summary>
        public Arcor2Client Client {
            get;
        } = new(logger: new Arcor2UnityLogger());

        /// <summary>
        /// Restarts the client into initial state, without unregistering event handlers.
        /// </summary>
        public void ReloadClient() {
            Client.Reset();
        }

        /// <summary>
        /// Creates new instance of <see cref="CommunicationManager"/> class.
        /// </summary>
        public CommunicationManager() {
            Client.ConnectionError += SafeEventHandler<Exception>(ClientOnConnectionError);
            Client.ConnectionClosed += SafeEventHandler<WebSocketCloseEventArgs>(ClientOnConnectionClosed);
            Client.ObjectsLocked += SafeEventHandler<ObjectsLockEventArgs>(ClientOnObjectsLocked);
            Client.ObjectsUnlocked += SafeEventHandler<ObjectsLockEventArgs>(ClientOnObjectsUnlocked);
            Client.SceneSaved += SafeEventHandler(ClientOnSceneSaved);
            Client.ProjectSaved += SafeEventHandler(ClientOnProjectSaved);
            Client.SceneClosed += SafeEventHandler(ClientOnSceneClosed);
            Client.ProjectClosed += SafeEventHandler(ClientOnProjectClosed);
            Client.SceneOpened += SafeEventHandler<OpenSceneEventArgs>(ClientOnSceneOpened);
            Client.ProjectOpened += SafeEventHandler<OpenProjectEventArgs>(ClientOnProjectOpened);
            Client.ObjectTypeAdded += SafeEventHandler<ObjectTypesEventArgs>(ClientOnObjectTypeAdded);
            Client.ObjectTypeUpdated += SafeEventHandler<ObjectTypesEventArgs>(ClientOnObjectTypeUpdated);
            Client.PackageInfo += SafeEventHandler<PackageInfoEventArgs>(ClientOnPackageInfo);
            Client.PackageState += SafeEventHandler<PackageStateEventArgs>(ClientOnPackageState);
            Client.PackageException += SafeEventHandler<PackageExceptionEventArgs>(ClientOnPackageException);
            Client.ActionExecution += SafeEventHandler<ActionExecutionEventArgs>(ClientOnActionExecution);
            Client.ActionCancelled += SafeEventHandler(ClientOnActionCancelled);
            Client.ActionResult += SafeEventHandler<ActionResultEventArgs>(ClientOnActionResult);
            Client.ActionStateAfter += SafeEventHandler<ActionStateAfterEventArgs>(ClientOnActionStateAfter);
            Client.ActionStateBefore += SafeEventHandler<ActionStateBeforeEventArgs>(ClientOnActionStateBefore);
            Client.ActionObjectAdded += SafeEventHandler<ActionObjectEventArgs>(ClientOnActionObjectAdded);
            Client.ActionObjectUpdated += SafeEventHandler<ActionObjectEventArgs>(ClientOnActionObjectUpdated);
            Client.ActionObjectRemoved += SafeEventHandler<ActionObjectEventArgs>(ClientOnActionObjectRemoved);
            Client.ActionAdded += SafeEventHandler<Arcor2.ClientSdk.Communication.ActionEventArgs>(ClientOnActionAdded);
            Client.ActionUpdated += SafeEventHandler<Arcor2.ClientSdk.Communication.ActionEventArgs>(ClientOnActionUpdated);
            Client.ActionBaseUpdated += SafeEventHandler<Arcor2.ClientSdk.Communication.ActionEventArgs>(ClientOnActionBaseUpdated);
            Client.ActionRemoved += SafeEventHandler<BareActionEventArgs>(ClientOnActionRemoved);

            RegisterProjectChangedEvents();
        }

        /// <summary>
        /// Sets project changed flag.
        /// </summary>
        private void RegisterProjectChangedEvents() {
            Client.JointsAdded += SafeEventHandler<JointsEventArgs>(SetProjectChanged);
            Client.JointsBaseUpdated += SafeEventHandler<JointsEventArgs>(SetProjectChanged);
            Client.JointsUpdated += SafeEventHandler<JointsEventArgs>(SetProjectChanged);
            Client.JointsRemoved += SafeEventHandler<JointsEventArgs>(SetProjectChanged);

            Client.OrientationAdded += SafeEventHandler<OrientationEventArgs>(SetProjectChanged);
            Client.OrientationBaseUpdated += SafeEventHandler<OrientationEventArgs>(SetProjectChanged);
            Client.OrientationUpdated += SafeEventHandler<OrientationEventArgs>(SetProjectChanged);
            Client.OrientationRemoved += SafeEventHandler<OrientationEventArgs>(SetProjectChanged);

            Client.ActionPointAdded += SafeEventHandler<BareActionPointEventArgs>(SetProjectChanged);
            Client.ActionPointBaseUpdated += SafeEventHandler<BareActionPointEventArgs>(SetProjectChanged);
            Client.ActionPointUpdated += SafeEventHandler<BareActionPointEventArgs>(SetProjectChanged);
            Client.ActionPointRemoved += SafeEventHandler<BareActionPointEventArgs>(SetProjectChanged);

            Client.ActionAdded += SafeEventHandler<Arcor2.ClientSdk.Communication.ActionEventArgs>(SetProjectChanged);
            Client.ActionBaseUpdated += SafeEventHandler<Arcor2.ClientSdk.Communication.ActionEventArgs>(SetProjectChanged);
            Client.ActionUpdated += SafeEventHandler<Arcor2.ClientSdk.Communication.ActionEventArgs>(SetProjectChanged);
            Client.ActionRemoved += SafeEventHandler<BareActionEventArgs>(SetProjectChanged);

            Client.ProjectBaseUpdated += SafeEventHandler<BareProjectEventArgs>(SetProjectChanged);
            Client.ProjectRemoved += SafeEventHandler<BareProjectEventArgs>(SetProjectChanged);

            Client.ProjectOverrideAdded += SafeEventHandler<ParameterEventArgs>(SetProjectChanged);
            Client.ProjectOverrideUpdated += SafeEventHandler<ParameterEventArgs>(SetProjectChanged);
            Client.ProjectOverrideRemoved += SafeEventHandler<ParameterEventArgs>(SetProjectChanged);

            Client.LogicItemAdded += SafeEventHandler<LogicItemEventArgs>(SetProjectChanged);
            Client.LogicItemUpdated += SafeEventHandler<LogicItemEventArgs>(SetProjectChanged);
            Client.LogicItemRemoved += SafeEventHandler<LogicItemEventArgs>(SetProjectChanged);
        }

        private void ClientOnActionObjectRemoved(object sender, ActionObjectEventArgs e) {
            SceneManager.Instance.SceneObjectRemoved(e.Data);
        }

        private void ClientOnActionObjectUpdated(object sender, ActionObjectEventArgs e) {
            SceneManager.Instance.SceneObjectUpdated(e.Data);
        }

        private void ClientOnActionObjectAdded(object sender, ActionObjectEventArgs e) {
            SceneManager.Instance.SceneObjectAdded(e.Data);
        }

        private void ClientOnActionRemoved(object sender, BareActionEventArgs e) {
            ProjectManager.Instance.ActionRemoved(e.Data);
        }

        private void ClientOnActionBaseUpdated(object sender, Arcor2.ClientSdk.Communication.ActionEventArgs e) {
            ProjectManager.Instance.ActionBaseUpdated(DataHelper.ActionToBareAction(e.Data));
        }

        private void ClientOnActionUpdated(object sender, Arcor2.ClientSdk.Communication.ActionEventArgs e) {
            ProjectManager.Instance.ActionUpdated(e.Data);
        }

        private void ClientOnActionAdded(object sender, Arcor2.ClientSdk.Communication.ActionEventArgs e) {
            ProjectManager.Instance.ActionAdded(e.Data, e.ParentId);
        }

        public static EventHandler<T> SafeEventHandler<T>(EventHandler<T> handler) {
            return (sender, args) => {
                MainThreadDispatcher.Enqueue(() => handler(sender, args));
            };
        }

        public static EventHandler SafeEventHandler(EventHandler handler) {
            return (sender, args) => {
                MainThreadDispatcher.Enqueue(() => handler(sender, args));
            };
        }

        private void ClientOnActionStateBefore(object sender, ActionStateBeforeEventArgs e) {
            try {

                if (!string.IsNullOrEmpty(e.Data.ActionId)) {
                    var puckId = e.Data.ActionId;

                    if (!ProjectManager.Instance.Valid) {
                        Debug.LogWarning("Project not yet loaded, ignoring current action");
                        GameManager.Instance.ActionRunningOnStartupId = puckId;
                        return;
                    }

                    if (ActionsManager.Instance.CurrentlyRunningAction != null)
                        ActionsManager.Instance.CurrentlyRunningAction.StopAction();

                    Action puck = ProjectManager.Instance.GetAction(puckId);
                    ActionsManager.Instance.CurrentlyRunningAction = puck;

                    puck.RunAction();
                } else {
                    if (ActionsManager.Instance.CurrentlyRunningAction != null) {
                        ActionsManager.Instance.CurrentlyRunningAction.StopAction();
                    }
                    ActionsManager.Instance.CurrentlyRunningAction = null;
                }
            } catch (ItemNotFoundException ex) {
                Debug.LogError(ex);
            }
        }

        private void ClientOnActionStateAfter(object sender, ActionStateAfterEventArgs e) {
            if (ProjectManager.Instance.Valid) {
                if (ActionsManager.Instance.CurrentlyRunningAction != null)
                    ActionsManager.Instance.CurrentlyRunningAction.StopAction();
                ActionsManager.Instance.CurrentlyRunningAction = null;
            }
        }

        private void ClientOnActionResult(object sender, ActionResultEventArgs e) {
            if (ProjectManager.Instance.Valid) {
                GameManager.Instance.HandleActionResult(e.Data);
            }
        }

        private void ClientOnActionCancelled(object sender, EventArgs e) {
            GameManager.Instance.HandleActionCanceled();
        }

        private void ClientOnActionExecution(object sender, ActionExecutionEventArgs e) {
            GameManager.Instance.HandleActionExecution(e.Data.ActionId);
        }

        private void ClientOnPackageException(object sender, PackageExceptionEventArgs e) {
            GameManager.Instance.HandleProjectException(e.Data);
        }

        private void ClientOnPackageState(object sender, PackageStateEventArgs e) {
            GameManager.Instance.PackageStateUpdated(e.Data);
        }

        private void ClientOnPackageInfo(object sender, PackageInfoEventArgs e) {
            GameManager.Instance.PackageInfo = e.Data;
        }

        private void ClientOnObjectTypeUpdated(object sender, ObjectTypesEventArgs e) {
            ActionsManager.Instance.ActionsReady = false;
        }

        private void ClientOnObjectTypeAdded(object sender, ObjectTypesEventArgs e) {
            ActionsManager.Instance.ActionsReady = false;
        }

        private void SetProjectChanged(object sender, EventArgs e) {
            ProjectManager.Instance.ProjectChanged = true;
        }

        private async void ClientOnSceneOpened(object sender, OpenSceneEventArgs e) {
            await GameManager.Instance.SceneOpened(e.Data.Scene);
        }

        private void ClientOnProjectOpened(object sender, OpenProjectEventArgs e) {
            GameManager.Instance.ProjectOpened(e.Data.Scene, e.Data.Project);
        }

        private void ClientOnProjectClosed(object sender, EventArgs e) {
            GameManager.Instance.ProjectClosed();
        }

        private void ClientOnSceneClosed(object sender, EventArgs e) {
            GameManager.Instance.SceneClosed();
        }

        private void ClientOnProjectSaved(object sender, EventArgs e) {
            ProjectManager.Instance.ProjectSaved();
        }

        private void ClientOnSceneSaved(object sender, EventArgs e) {
            SceneManager.Instance.SceneSaved();
        }

        private void ClientOnObjectsUnlocked(object sender, ObjectsLockEventArgs e) {
            LockingEventsCache.Instance.Add(new ObjectLockingEventArgs(e.Data.ObjectIds, false, e.Data.Owner));
        }

        private void ClientOnObjectsLocked(object sender, ObjectsLockEventArgs e) {
            LockingEventsCache.Instance.Add(new ObjectLockingEventArgs(e.Data.ObjectIds, true, e.Data.Owner));
        }

        private void ClientOnConnectionClosed(object sender, WebSocketCloseEventArgs e) {
            GameManager.Instance.HideLoadingScreen();
            GameManager.Instance.ConnectionStatus = GameManager.ConnectionStatusEnum.Disconnected;
            ReloadClient();
        }

        private void ClientOnConnectionError(object sender, Exception e) {
            Debug.Log(e.Message);
        }

        private class Arcor2UnityLogger : IArcor2Logger {
            public void LogInfo(string message) {
                Debug.Log(message);
            }

            public void LogError(string message) {
                Debug.LogError(message);
            }

            public void LogWarning(string message) {
                Debug.LogWarning(message);
            }
        }
    }
}
