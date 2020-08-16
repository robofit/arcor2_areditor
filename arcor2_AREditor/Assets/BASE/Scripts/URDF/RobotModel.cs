using System.Collections;
using System.Collections.Generic;
using RosSharp;
using RosSharp.RosBridgeClient;
using RosSharp.Urdf;
using UnityEngine;

public class RobotModel {

    public string RobotType { get; private set; }
    public GameObject RobotModelGameObject { get; private set; }

    public bool IsBeingUsed { get; set; }

    public Dictionary<string, RobotLink> Links = new Dictionary<string, RobotLink>();
    public Dictionary<string, string> Joints = new Dictionary<string, string>();

    private bool robotLoaded = false;

    public RobotModel(string robotType, GameObject robotModel) {
        RobotType = robotType;
        RobotModelGameObject = robotModel;
        IsBeingUsed = false;
    }
    
    /// <summary>
    /// Initializes RobotLinks and sets a boolean to its Visuals dictionary,
    /// telling whether the model of individual visual was already imported (is type of box, cylinder, capsule)
    /// or not yet (is mesh - is going to be continually imported from ColladaImporter).
    /// </summary>
    public void LoadLinks() {
        // Get all UrdfLink components in builded Robot
        foreach (UrdfLink link in RobotModelGameObject.GetComponentsInChildren<UrdfLink>()) {

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

            UrdfJoint urdfJoint = link.GetComponent<UrdfJoint>();
            JointStateWriter jointWriter = null;
            if (urdfJoint != null) {
                if (urdfJoint.JointType != UrdfJoint.JointTypes.Fixed) {
                    jointWriter = urdfJoint.transform.AddComponentIfNotExists<JointStateWriter>();
                    Joints.Add(urdfJoint.JointName, link.gameObject.name);
                }
            }
            Links.Add(link.gameObject.name, new RobotLink(link.gameObject.name, urdfJoint, jointWriter, visuals, is_base_link: link.IsBaseLink));
        }
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
        }
        robotLoaded = true;
        OnRobotLoaded();

        return true;
    }

    private void OnRobotLoaded() {
        Debug.Log("URDF: robot is fully loaded");

        SetActiveAllVisuals(true);

        UrdfManager.Instance.RobotModelLoaded(RobotType);

        //// if robot is loaded, unsubscribe from ColladaImporter event, for performance efficiency
        //ColladaImporter.Instance.OnModelImported -= OnColladaModelImported;

        //outlineOnClick.ClearRenderers();
        //RobotPlaceholder.SetActive(false);
        //Destroy(RobotPlaceholder);

        //robotColliders.Clear();
        //robotRenderers.Clear();
        //robotRenderers.AddRange(RobotModel.GetComponentsInChildren<Renderer>());
        //robotColliders.AddRange(RobotModel.GetComponentsInChildren<Collider>());
        //outlineOnClick.InitRenderers(robotRenderers);
        //outlineOnClick.OutlineShaderType = OutlineOnClick.OutlineType.TwoPassShader;
    }


    /// <summary>
    /// Displays or hides all visuals of the robot.
    /// </summary>
    /// <param name="active"></param>
    public void SetActiveAllVisuals(bool active) {
        foreach (RobotLink link in Links.Values) {
            link.SetActiveVisuals(active);
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
    /// <param name="jointName"></param>
    /// <param name="angle"></param>
    public void SetJointAngle(string jointName, float angle) {
        if (robotLoaded) {
            Joints.TryGetValue(jointName, out string linkName);
            Links.TryGetValue(linkName, out RobotLink link);
            //Debug.Log(jointName + " ..angle in deg: " + angle + " ..angle in rad: " + angle * Mathf.Deg2Rad);
            angle *= Mathf.Deg2Rad;
            link?.SetJointAngle(angle);
        }
    }
}
