using System;
using System.Collections;
using System.Collections.Generic;
using RosSharp.RosBridgeClient;
using UnityEngine;

public class RobotLink {

    public string LinkName {
        get;
    }

    public List<GameObject> Visuals {
        get;
        private set;
    }

    public bool IsBaseLink {
        get;
    }

    private JointStateWriter jointWriter;

    public RobotLink(string link_name, JointStateWriter joint_writer, List<GameObject> visuals_gameObject = null, bool is_base_link = false) {
        LinkName = link_name;
        jointWriter = joint_writer;
        Visuals = visuals_gameObject ?? new List<GameObject>();
        IsBaseLink = is_base_link;
    }
    public void SetJointAngle(float angle) {
        if (jointWriter != null) {
            jointWriter.Write(angle);
        }
    }

    public void AddVisual(GameObject visual) {
        Visuals.Add(visual);
    }

    public void SetActiveVisuals(bool active) {
        foreach (GameObject visual in Visuals) {
            visual.SetActive(active);
        }
    }

    public bool HasVisuals() {
        return Visuals.Count > 0 ? true : false;
    }
}
