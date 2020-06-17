using System;
using System.Collections;
using System.Collections.Generic;
using RosSharp.RosBridgeClient;
using RosSharp.Urdf;
using UnityEngine;

public class RobotLink {

    public string LinkName {
        get;
    }

    public Dictionary<UrdfVisual, bool> Visuals {
        get;
        private set;
    }

    public bool IsBaseLink {
        get;
    }

    public UrdfJoint UrdfJoint {
        get;
    }

    private JointStateWriter jointWriter;

    public RobotLink(string link_name, UrdfJoint urdf_joint, JointStateWriter joint_writer, Dictionary<UrdfVisual, bool> visuals_gameObject = null, bool is_base_link = false) {
        LinkName = link_name;
        UrdfJoint = urdf_joint;
        jointWriter = joint_writer;
        Visuals = visuals_gameObject ?? new Dictionary<UrdfVisual, bool>();
        IsBaseLink = is_base_link;
    }

    public void SetJointAngle(float angle) {
        if (jointWriter != null) {
            jointWriter.Write(angle);
        }
    }

    public void SetVisualLoaded(UrdfVisual urdfVisual) {
        Visuals[urdfVisual] = true;
    }

    public void SetActiveVisuals(bool active) {
        foreach (UrdfVisual visual in Visuals.Keys) {
            visual.gameObject.SetActive(active);
        }
    }

    public bool HasVisualsLoaded() {
        foreach (bool visualLoaded in Visuals.Values) {
            if (!visualLoaded) {
                return false;
            }
        }
        return true;
    }
}
