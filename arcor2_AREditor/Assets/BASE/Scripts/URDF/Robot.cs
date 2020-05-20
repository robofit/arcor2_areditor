using System.Collections;
using System.Collections.Generic;
using RosSharp.RosBridgeClient;
using RosSharp.Urdf;
using UnityEngine;

namespace Base {
    public class Robot : MonoBehaviour {

        public Dictionary<string, RobotLink> Links = new Dictionary<string, RobotLink>();

        private bool robotLoaded = false;

        public void LoadLinks() {
            foreach (UrdfLink link in GetComponentsInChildren<UrdfLink>()) {
                if (link.IsBaseLink || link.gameObject.name.ToLower().Contains("base_link") || link.gameObject.name.ToLower().Contains("baselink")) {
                    Links.Add(link.gameObject.name, new RobotLink(link.gameObject.name, null, is_base_link:true));
                } else {
                    JointStateWriter jointWriter = link.gameObject.AddComponent<JointStateWriter>();
                    Links.Add(link.gameObject.name, new RobotLink(link.gameObject.name, jointWriter));
                }
            }
        }

        public void SetRandomJointAngles() {
            foreach (RobotLink link in Links.Values) {
                link.SetJointAngle(Random.Range(-6.28f, 6.28f));
            }
        }

        public void SetJointAngle(string linkName, float angle) {
            Links.TryGetValue(linkName, out RobotLink link);
            link?.SetJointAngle(angle);
        }

        public void AddLinkVisual(string linkName, GameObject visual) {
            Links.TryGetValue(linkName, out RobotLink link);
            link?.AddVisual(visual);

            IsRobotLoaded();

            // if robot is loaded, show its visuals, otherwise hide them
            link?.SetActiveVisuals(robotLoaded);
        }

        private bool IsRobotLoaded() {
            if (!robotLoaded) {
                foreach (RobotLink link in Links.Values) {
                    if (!link.HasVisuals()) {
                        return false;
                    }
                }
                robotLoaded = true;
            }
            SetActiveAllVisuals(true);
            return true;
        }

        private void SetActiveAllVisuals(bool active) {
            foreach (RobotLink link in Links.Values) {
                link.SetActiveVisuals(active);
            }
        }

    }
}
