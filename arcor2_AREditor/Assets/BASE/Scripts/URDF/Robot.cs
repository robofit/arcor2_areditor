using System.Collections;
using System.Collections.Generic;
using System.IO;
using RosSharp.RosBridgeClient;
using RosSharp.Urdf;
using RosSharp.Urdf.Runtime;
using UnityEngine;

namespace Base {
    public class Robot : ActionObject {

        public Dictionary<string, RobotLink> Links = new Dictionary<string, RobotLink>();

        private bool robotLoaded = false;

        protected override void Start() {
            base.Start();
            SceneManager.Instance.OnUrdfReady += OnUrdfDownloaded;
        }

        public override void InitActionObject(string id, string type, Vector3 position, Quaternion orientation, string uuid, ActionObjectMetadata actionObjectMetadata, IO.Swagger.Model.CollisionModels customCollisionModels = null) {
            base.InitActionObject(id, type, position, orientation, uuid, actionObjectMetadata);
            Data.Id = id;
            Data.Type = type;
            SetScenePosition(position);
            SetSceneOrientation(orientation);
            Data.Id = uuid;
            ActionObjectMetadata = actionObjectMetadata;
            // TODO - create basic cube and replace it later with robot model
            //CreateModel(customCollisionModels);
            enabled = true;
            SetVisibility(visibility);
        }

        private void OnUrdfDownloaded(object sender, RobotUrdfArgs args) {
            // check if downloaded urdf contains this robot
            if (ActionObjectMetadata.Type == args.RobotType) {
                DirectoryInfo dir = new DirectoryInfo(args.Path);
                FileInfo[] files = dir.GetFiles("*.urdf", SearchOption.TopDirectoryOnly);

                // if .urdf is missing, try to find .xml file
                if (files.Length == 0) {
                    files = dir.GetFiles("*.xml", SearchOption.TopDirectoryOnly);
                }

                // import only first found file
                if (files.Length > 0) {
                    ImportUrdfObject(files[0].FullName);

                    // subscribe for ColladaImporter event in order to load robot links
                    ColladaImporter.Instance.OnModelImported += OnColladaModelImported;
                }
            }
        }

        /// <summary>
        /// Imports URDF based on a given filename. Filename has to contain a full path.
        /// </summary>
        /// <param name="filename"></param>
        private void ImportUrdfObject(string filename) {
            UrdfRobot urdfRobot = UrdfRobotExtensionsRuntime.Create(filename, useUrdfMaterials: false);
            urdfRobot.transform.parent = transform;
            urdfRobot.transform.localPosition = Vector3.zero;

            urdfRobot.SetRigidbodiesIsKinematic(true);

            LoadLinks();
        }

        private void OnColladaModelImported(object sender, ImportedColladaEventArgs args) {
            Transform importedModel = args.Data.transform;

            Base.Robot robot = importedModel.GetComponentInParent<Base.Robot>();
            if (robot != null) {
                // check if imported model corresponds to this robot
                if (ReferenceEquals(robot, this)) {

                    // get rid of the placeholder object (New Game Object)
                    Transform placeholderGameObject = importedModel.parent;
                    importedModel.SetParent(placeholderGameObject.parent, false);

                    //TODO: Temporarily, colliders are added directly to Visuals
                    AddColliders(importedModel.gameObject, setConvex: true);

                    Destroy(placeholderGameObject.gameObject);

                    SetLinkVisualLoaded(importedModel.parent.parent.parent.name, importedModel.parent.gameObject.GetComponent<UrdfVisual>());
                }
            }
        }

        private static void AddColliders(GameObject gameObject, bool setConvex = false) {
            MeshFilter[] meshFilters = gameObject.GetComponentsInChildren<MeshFilter>();
            foreach (MeshFilter meshFilter in meshFilters) {
                GameObject child = meshFilter.gameObject;
                MeshCollider meshCollider = child.AddComponent<MeshCollider>();
                meshCollider.sharedMesh = meshFilter.sharedMesh;

                meshCollider.convex = setConvex;
            }
        }

        /// <summary>
        /// Initializes RobotLinks and sets a boolean to its Visuals dictionary,
        /// telling whether the model of individual visual was already imported (is type of box, cylinder, capsule)
        /// or not yet (is mesh - is going to be continually imported from ColladaImporter).
        /// </summary>
        private void LoadLinks() {
            // Get all UrdfLink components in builded Robot
            foreach (UrdfLink link in GetComponentsInChildren<UrdfLink>()) {

                // Get all UrdfVisuals of each UrdfLink
                GameObject visualsGameObject = link.gameObject.GetComponentInChildren<UrdfVisuals>().gameObject;
                Dictionary<UrdfVisual, bool> visuals = new Dictionary<UrdfVisual, bool>();
                // Traverse each UrdfVisual and set a boolean indicating whether its visual is already loaded (is of some basic type - box, cylinder, capsule)
                // or is going to be loaded by ColladaImporter (in case its type of mesh)
                foreach (UrdfVisual visual in visualsGameObject.GetComponentsInChildren<UrdfVisual>()) {
                    visuals.Add(visual, visual.GeometryType == GeometryTypes.Mesh ? false : true);
                    // hide visual if it is mesh.. mesh will be displayed when fully loaded
                    visual.gameObject.SetActive(visual.GeometryType == GeometryTypes.Mesh ? false : true);
                }

                // Distinguish between base links (do not have joints, so no JointStateWriter will be included) and normal links (do have joints, so JointStateWriter will be inculed)
                if (link.IsBaseLink || link.gameObject.name.ToLower().Contains("base_link") || link.gameObject.name.ToLower().Contains("baselink")) {  
                    Links.Add(link.gameObject.name, new RobotLink(link.gameObject.name, null, visuals, is_base_link: true));
                } else {
                    JointStateWriter jointWriter = link.gameObject.AddComponent<JointStateWriter>();
                    Links.Add(link.gameObject.name, new RobotLink(link.gameObject.name, jointWriter, visuals));
                }
            }
        }

        public void SetRandomJointAngles() {
            foreach (RobotLink link in Links.Values) {
                link.SetJointAngle(Random.Range(-6.28f, 6.28f));
            }
        }

        /// <summary>
        /// Sets angle of joint in given linkName.
        /// </summary>
        /// <param name="linkName"></param>
        /// <param name="angle"></param>
        public void SetJointAngle(string linkName, float angle) {
            Links.TryGetValue(linkName, out RobotLink link);
            link?.SetJointAngle(angle);
        }

        /// <summary>
        /// Sets that visual of a given link is loaded (ColladaImporter imported mesh).
        /// </summary>
        /// <param name="linkName"></param>
        /// <param name="urdfVisual"></param>
        public void SetLinkVisualLoaded(string linkName, UrdfVisual urdfVisual) {
            Links.TryGetValue(linkName, out RobotLink link);
            link?.SetVisualLoaded(urdfVisual);

            IsRobotLoaded();

            // if robot is loaded, show its visuals, otherwise hide them
            link?.SetActiveVisuals(robotLoaded);
        }

        /// <summary>
        /// Checks that all visuals (meshes, primitive types - box, cylinder..) of the robot are imported and created.
        /// </summary>
        /// <returns></returns>
        private bool IsRobotLoaded() {
            if (!robotLoaded) {
                foreach (RobotLink link in Links.Values) {
                    if (!link.HasVisualsLoaded()) {
                        return false;
                    }
                }
                robotLoaded = true;

                // if robot is loaded, unsubscribe from ColladaImporter event, for performance efficiency
                ColladaImporter.Instance.OnModelImported -= OnColladaModelImported;
            }
            SetActiveAllVisuals(true);
            return true;
        }

        /// <summary>
        /// Displays or hides all visuals of the robot.
        /// </summary>
        /// <param name="active"></param>
        private void SetActiveAllVisuals(bool active) {
            foreach (RobotLink link in Links.Values) {
                link.SetActiveVisuals(active);
            }
        }

        public override Vector3 GetScenePosition() {
            return TransformConvertor.ROSToUnity(DataHelper.PositionToVector3(Data.Pose.Position));
        }

        public override void SetScenePosition(Vector3 position) {
            Data.Pose.Position = DataHelper.Vector3ToPosition(TransformConvertor.UnityToROS(position));
        }

        public override Quaternion GetSceneOrientation() {
            return TransformConvertor.ROSToUnity(DataHelper.OrientationToQuaternion(Data.Pose.Orientation));
        }

        public override void SetSceneOrientation(Quaternion orientation) {
            Data.Pose.Orientation = DataHelper.QuaternionToOrientation(TransformConvertor.UnityToROS(orientation));
        }

        public override void Show() {

        }

        public override void Hide() {

        }

        public override void SetInteractivity(bool interactive) {

        }

        public override void OnClick(Click type) {

        }
    }
}
