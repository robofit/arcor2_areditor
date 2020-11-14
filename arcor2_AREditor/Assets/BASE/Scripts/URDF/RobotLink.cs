using System;
using System.Collections;
using System.Collections.Generic;
using Base;
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

    public Dictionary<UrdfCollision, bool> Collisions {
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

    public RobotLink(string link_name, UrdfJoint urdf_joint, JointStateWriter joint_writer, Dictionary<UrdfVisual, bool> visuals_gameObject = null, Dictionary<UrdfCollision, bool> collisions_gameObject = null, bool is_base_link = false) {
        LinkName = link_name;
        UrdfJoint = urdf_joint;
        jointWriter = joint_writer;
        Visuals = visuals_gameObject ?? new Dictionary<UrdfVisual, bool>();
        Collisions = collisions_gameObject ?? new Dictionary<UrdfCollision, bool>();
        IsBaseLink = is_base_link;
    }

    public void SetJointAngle(float angle) {
        if (jointWriter != null) {
            jointWriter.Write(angle);
        }
    }

    public decimal GetJointAngle() {
        if (jointWriter != null) {
            return Convert.ToDecimal(jointWriter.GetCurrentValue());
        } else {
            throw new RequestFailedException("Unable to read current joints angles");
        }
    }

    public void SetVisualLoaded(UrdfVisual urdfVisual) {
        Visuals[urdfVisual] = true;
    }

    public void SetCollisionLoaded(UrdfCollision urdfCollision) {
        Collisions[urdfCollision] = true;
    }

    public void SetActiveVisuals(bool active) {
        foreach (UrdfVisual visual in Visuals.Keys) {
            visual.gameObject.SetActive(active);
        }
    }

    public void SetActiveCollisions(bool active) {
        foreach (UrdfCollision collision in Collisions.Keys) {
            collision.gameObject.SetActive(active);
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

    public bool HasCollisionsLoaded() {
        foreach (bool collisionLoaded in Collisions.Values) {
            if (!collisionLoaded) {
                return false;
            }
        }
        return true;
    }
}
