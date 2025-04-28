using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Arcor2.ClientSdk.Communication;
using Arcor2.ClientSdk.Communication.OpenApi.Models;
using UnityEngine;
using Pose = Arcor2.ClientSdk.Communication.OpenApi.Models.Pose;

namespace Base {
    public abstract class ActionObject : InteractiveObject, IActionProvider, IActionPointParent {

        public GameObject ActionPointsSpawn;
        [NonSerialized]
        public int CounterAP = 0;
        protected float visibility;

        public Collider Collider;

        public SceneObject Data = new(id: "", name: "", pose: DataHelper.CreatePose(new Vector3(), new Quaternion()), type: "");
        public ActionObjectMetadata ActionObjectMetadata;

        public Dictionary<string, Parameter> ObjectParameters = new();
        public Dictionary<string, Parameter> Overrides = new();


        public virtual void InitActionObject(SceneObject sceneObject, Vector3 position, Quaternion orientation, ActionObjectMetadata actionObjectMetadata, CollisionModels customCollisionModels = null, bool loadResuources = true) {
            Data.Id = sceneObject.Id;
            Data.Type = sceneObject.Type;
            name = sceneObject.Name; // show actual object name in unity hierarchy
            ActionObjectMetadata = actionObjectMetadata;
            if (actionObjectMetadata.HasPose) {
                SetScenePosition(position);
                SetSceneOrientation(orientation);
            }
            CreateModel(customCollisionModels);
            enabled = true;
            SelectorItem = SelectorMenu.Instance.CreateSelectorItem(this);
            if (VRModeManager.Instance.VRModeON) {
                SetVisibility(PlayerPrefsHelper.LoadFloat("AOVisibilityVR", 1f));
            } else {
                SetVisibility(PlayerPrefsHelper.LoadFloat("AOVisibilityAR", 0f));
            }

            if (PlayerPrefsHelper.LoadBool($"ActionObject/{GetId()}/blocklisted", false)) {
                Enable(false, true, false);
            }

        }

        public virtual void UpdateObjectName(string newUserId) {
            Data.Name = newUserId;
            SelectorItem.SetText(newUserId);
        }

        protected virtual void Update() {
            if (ActionObjectMetadata != null && ActionObjectMetadata.HasPose && gameObject.transform.hasChanged) {
                transform.hasChanged = false;
            }
        }

        public virtual void ActionObjectUpdate(SceneObject actionObjectSwagger) {
            if ((Data != null) & (Data.Name != actionObjectSwagger.Name))
                UpdateObjectName(actionObjectSwagger.Name);
            Data = actionObjectSwagger;
            foreach (Arcor2.ClientSdk.Communication.OpenApi.Models.Parameter p in Data.Parameters) {
                if (!ObjectParameters.ContainsKey(p.Name)) {
                    if (TryGetParameterMetadata(p.Name, out ParameterMeta parameterMeta)) {
                        ObjectParameters[p.Name] = new Parameter(parameterMeta, p.Value);
                    } else {
                        Debug.LogError("Failed to load metadata for parameter " + p.Name);
                        Notifications.Instance.ShowNotification("Critical error", "Failed to load parameter's metadata.");
                        return;
                    }

                } else {
                    ObjectParameters[p.Name].Value = p.Value;
                }

            }

        }

        public void ResetPosition() {
            transform.localPosition = GetScenePosition();
            transform.localRotation = GetSceneOrientation();
        }

        public virtual bool SceneInteractable() {
            return GameManager.Instance.GetGameState() == GameManager.GameStateEnum.SceneEditor;
        }

        public bool TryGetParameter(string id, out Arcor2.ClientSdk.Communication.OpenApi.Models.Parameter parameter) {
            foreach (Arcor2.ClientSdk.Communication.OpenApi.Models.Parameter p in Data.Parameters) {
                if (p.Name == id) {
                    parameter = p;
                    return true;
                }
            }
            parameter = null;
            return false;
        }

        public bool TryGetParameterMetadata(string id, out ParameterMeta parameterMeta) {
            foreach (ParameterMeta p in ActionObjectMetadata.Settings) {
                if (p.Name == id) {
                    parameterMeta = p;
                    return true;
                }
            }
            parameterMeta = null;
            return false;
        }

        public abstract Vector3 GetScenePosition();

        public abstract void SetScenePosition(Vector3 position);

        public abstract Quaternion GetSceneOrientation();

        public abstract void SetSceneOrientation(Quaternion orientation);

        public string GetProviderName() {
            return Data.Name;
        }


        public ActionMetadata GetActionMetadata(string action_id) {
            if (ActionObjectMetadata.ActionsLoaded) {
                if (ActionObjectMetadata.ActionsMetadata.TryGetValue(action_id, out ActionMetadata actionMetadata)) {
                    return actionMetadata;
                } else {
                    throw new ItemNotFoundException("Metadata not found");
                }
            }
            return null; //TODO: throw exception
        }


        public bool IsRobot() {
            return ActionObjectMetadata.Robot;
        }

        public bool IsCamera() {
            return ActionObjectMetadata.Camera;
        }

        public virtual void DeleteActionObject() {
            // Remove all actions of this action point
            RemoveActionPoints();

            // Remove this ActionObject reference from the scene ActionObject list
            SceneManager.Instance.ActionObjects.Remove(Data.Id);

            DestroyObject();
            Destroy(gameObject);
        }

        public override void DestroyObject() {
            base.DestroyObject();
        }

        public void RemoveActionPoints() {
            // Remove all action points of this action object
            List<ActionPoint> actionPoints = GetActionPoints();
            foreach (ActionPoint actionPoint in actionPoints) {
                actionPoint.DeleteAP();
            }
        }


        public virtual void SetVisibility(float value, bool forceShaderChange = false) {
            visibility = value;
        }

        public float GetVisibility() {
            return visibility;
        }

        public abstract void Show();

        public abstract void Hide();

        public abstract void SetInteractivity(bool interactive);


        public virtual void ActivateForGizmo(string layer) {
            gameObject.layer = LayerMask.NameToLayer(layer);
        }

        public string GetProviderId() {
            return Data.Id;
        }

        public abstract void UpdateModel();

        public List<ActionPoint> GetActionPoints() {
            List<ActionPoint> actionPoints = new();
            foreach (ActionPoint actionPoint in ProjectManager.Instance.ActionPoints.Values) {
                if (actionPoint.Data.Parent == Data.Id) {
                    actionPoints.Add(actionPoint);
                }
            }
            return actionPoints;
        }

        public override string GetName() {
            return Data.Name;
        }


        public bool IsActionObject() {
            return true;
        }

        public ActionObject GetActionObject() {
            return this;
        }


        public Transform GetTransform() {
            return transform;
        }

        public string GetProviderType() {
            return Data.Type;
        }

        public GameObject GetGameObject() {
            return gameObject;
        }

        public override string GetId() {
            return Data.Id;
        }

        public async override Task<RequestResult> Movable() {
            if (!ActionObjectMetadata.HasPose)
                return new RequestResult(false, "Selected action object has no pose");
            else if (GameManager.Instance.GetGameState() != GameManager.GameStateEnum.SceneEditor) {
                return new RequestResult(false, "Action object could be moved only in scene editor");
            } else {
                return new RequestResult(true);
            }
        }

        public abstract void CreateModel(CollisionModels customCollisionModels = null);
        public abstract GameObject GetModelCopy();

        public Pose GetPose() {
            if (ActionObjectMetadata.HasPose)
                return new Pose(DataHelper.Vector3ToPosition(TransformConvertor.UnityToROS(transform.localPosition)),
                    DataHelper.QuaternionToOrientation(TransformConvertor.UnityToROS(transform.localRotation)));
            else
                return new Pose(orientation: new Orientation(), position: new Position());
        }
        public async override Task Rename(string name) {
            try {
                await CommunicationManager.Instance.Client.RenameActionObjectAsync(new RenameArgs(GetId(), name));
                Notifications.Instance.ShowToastMessage("Action object renamed");
            } catch (Arcor2ConnectionException e) {
                Notifications.Instance.ShowNotification("Failed to rename action object", e.Message);
                throw;
            }
        }
        public async override Task<RequestResult> Removable() {
            if (GameManager.Instance.GetGameState() != GameManager.GameStateEnum.SceneEditor) {
                return new RequestResult(false, "Action object could be removed only in scene editor");
            } else if (SceneManager.Instance.SceneStarted) {
                return new RequestResult(false, "Scene online");
            } else {
                RemoveFromSceneResponse response = await CommunicationManager.Instance.Client.RemoveActionObjectFromSceneAsync(new RemoveFromSceneRequestArgs(GetId(), false), true);
                if (response.Result)
                    return new RequestResult(true);
                else
                    return new RequestResult(false, response.Messages[0]);
            }
        }


        public async override void Remove() {
            RemoveFromSceneResponse response =
            await CommunicationManager.Instance.Client.RemoveActionObjectFromSceneAsync(new RemoveFromSceneRequestArgs(GetId(), false));
            if (!response.Result) {
                Notifications.Instance.ShowNotification("Failed to remove object " + GetName(), response.Messages[0]);
                return;
            }
        }

        public Transform GetSpawnPoint() {
            return transform;
        }
    }


}
