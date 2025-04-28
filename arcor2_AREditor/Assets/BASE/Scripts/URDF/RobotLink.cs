using System;
using System.Collections.Generic;
using Arcor2.ClientSdk.Communication;
using RosSharp.RosBridgeClient;
using RosSharp.Urdf;

public class RobotLink {

    public string LinkName {
        get;
    }

    public Dictionary<UrdfVisual, bool> Visuals {
        get;
    }

    public Dictionary<UrdfCollision, bool> Collisions {
        get;
    }

    public bool IsBaseLink {
        get;
    }

    public UrdfJoint UrdfJoint {
        get;
    }

    private JointStateWriter jointWriter;
    private JointStateReader jointReader;

    public float LinkScale {
        get;
        private set;
    }


    public RobotLink(string link_name, UrdfJoint urdf_joint, JointStateWriter joint_writer, JointStateReader joint_reader, Dictionary<UrdfVisual, bool> visuals_gameObject = null, Dictionary<UrdfCollision, bool> collisions_gameObject = null, bool is_base_link = false, float scale = 1f) {
        LinkName = link_name;
        UrdfJoint = urdf_joint;
        jointWriter = joint_writer;
        jointReader = joint_reader;
        Visuals = visuals_gameObject ?? new Dictionary<UrdfVisual, bool>();
        Collisions = collisions_gameObject ?? new Dictionary<UrdfCollision, bool>();
        IsBaseLink = is_base_link;
        LinkScale = scale;
    }

    public void SetJointAngle(float angle) {
        if (jointWriter != null) {
            jointWriter.Write(angle);
        }
    }

    public decimal GetJointAngle() {
        if (jointReader != null) {
            jointReader.Read(out string name, out float position, out float velocity, out float effort);
            return Convert.ToDecimal(position);
        } else {
            throw new Arcor2ConnectionException("Unable to read current joints angles");
        }
    }

    public void SetVisualLoaded(UrdfVisual urdfVisual) {
        Visuals[urdfVisual] = true;
    }

    public void SetLinkScale(float scale) {
        LinkScale = scale;
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
