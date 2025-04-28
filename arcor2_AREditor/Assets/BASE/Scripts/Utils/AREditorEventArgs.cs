using System;
using System.Collections.Generic;
using UnityEngine;

namespace Base {

    public class ActionEventArgs : EventArgs {

        public Action Action {
            get; set;
        }

        public ActionEventArgs(Action action) {
            Action = action;
        }
    }
    public class ActionPointEventArgs : EventArgs {

        public ActionPoint ActionPoint {
            get; set;
        }

        public ActionPointEventArgs(ActionPoint actionPoint) {
            ActionPoint = actionPoint;
        }
    }
    public class FloatEventArgs : EventArgs {
        public float Data {
            get; set;
        }

        public FloatEventArgs(float data) {
            Data = data;
        }
    }
    public class RobotUrdfModelArgs : EventArgs {

        public string RobotType {
            get; set;
        }

        public RobotUrdfModelArgs(string robotType) {
            RobotType = robotType;
        }
    }
    public class GameStateEventArgs : EventArgs {
        public GameManager.GameStateEnum Data {
            get;
            set;
        }

        public GameStateEventArgs(GameManager.GameStateEnum data) {
            Data = data;
        }
    }

    public class EditorStateEventArgs : EventArgs {
        public GameManager.EditorStateEnum Data {
            get;
            set;
        }

        public EditorStateEventArgs(GameManager.EditorStateEnum data) {
            Data = data;
        }
    }

    public class InteractiveObjectEventArgs : EventArgs {
        public InteractiveObject InteractiveObject {
            get;
            set;
        }

        public InteractiveObjectEventArgs(InteractiveObject interactiveObject) {
            InteractiveObject = interactiveObject;
        }
    }
    public class CalibrationEventArgs : EventArgs {

        public bool Calibrated {
            get;
            set;
        }

        public GameObject Anchor {
            get;
            set;
        }

        public CalibrationEventArgs(bool calibrated, GameObject anchor) {
            Calibrated = calibrated;
            Anchor = anchor;
        }
    }
    public class StringEventArgs : EventArgs {
        public string Data {
            get;
            set;
        }

        public StringEventArgs(string data) {
            Data = data;
        }
    }

    public class ObjectLockingEventArgs : EventArgs {
        public List<string> ObjectIds {
            get;
            set;
        }

        public bool Locked {
            get;
            set;
        }

        public string Owner {
            get;
            set;
        }

        public ObjectLockingEventArgs(List<string> objectIds, bool locked, string owner) {
            ObjectIds = objectIds;
            Locked = locked;
            Owner = owner;
        }
    }

    public class ProjectMetaEventArgs : EventArgs {
        public string Name {
            get;
            set;
        }

        public string Id {
            get;
            set;
        }

        public ProjectMetaEventArgs(string id, string name) {
            Id = id;
            Name = name;
        }
    }

    public class GizmoAxisEventArgs : EventArgs {
        public Gizmo.Axis SelectedAxis {
            get; set;
        }

        public GizmoAxisEventArgs(Gizmo.Axis gizmoAxis) {
            SelectedAxis = gizmoAxis;
        }
    }
}
