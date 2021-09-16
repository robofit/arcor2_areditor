using System.Collections.Generic;
using ARServer.Models;
using IO.Swagger.Model;
using UnityEngine;
using UnityEngine.PlayerLoop;

namespace Base {
    public class ActionObjectMetadata : ObjectTypeMeta {

        private Dictionary<string, ActionMetadata> actionsMetadata = new Dictionary<string, ActionMetadata>();
        private bool robot, actionsLoaded, camera, collisionObject;

        public ActionObjectMetadata(ObjectTypeMeta meta) : base(_abstract: meta.Abstract,
                                                                _base: meta.Base,
                                                                builtIn: meta.BuiltIn,
                                                                description: meta.Description,
                                                                disabled: meta.Disabled,
                                                                hasPose: meta.HasPose,
                                                                needsParentType: meta.NeedsParentType,
                                                                objectModel: meta.ObjectModel,
                                                                problem: meta.Problem,
                                                                settings: meta.Settings,
                                                                type: meta.Type) {
           
        }

        public void Update(ObjectTypeMeta objectTypeMeta) {
            Abstract = objectTypeMeta.Abstract;
            Base = objectTypeMeta.Base;
            BuiltIn = objectTypeMeta.BuiltIn;
            Description = objectTypeMeta.Description;
            HasPose = objectTypeMeta.HasPose;
            NeedsParentType = objectTypeMeta.NeedsParentType;
            ObjectModel = objectTypeMeta.ObjectModel;
            Problem = objectTypeMeta.Problem;
            Settings = objectTypeMeta.Settings;
        }

        public bool Robot {
            get => robot;
            set => robot = value;
        }

        public bool Camera {
            get => camera;
            set => camera = value;
        }

        public bool ActionsLoaded {
            get => actionsLoaded;
            set => actionsLoaded = value;
        }
        public Dictionary<string, ActionMetadata> ActionsMetadata {
            get => actionsMetadata;
            set => actionsMetadata = value;
        }
        public bool CollisionObject {
            get => collisionObject;
            set => collisionObject = value;
        }

        public Vector3 GetModelBB() {
            if (ObjectModel == null)
                return new Vector3(0.05f, 0.01f, 0.05f);
            switch (ObjectModel.Type) {
                case ObjectModel.TypeEnum.Box:
                    return new Vector3((float) ObjectModel.Box.SizeX, (float) ObjectModel.Box.SizeY, (float) ObjectModel.Box.SizeZ);
                case ObjectModel.TypeEnum.Cylinder:
                    return new Vector3((float) ObjectModel.Cylinder.Radius, (float) ObjectModel.Cylinder.Height, (float) ObjectModel.Cylinder.Radius);
                case ObjectModel.TypeEnum.Sphere:
                    return new Vector3((float) ObjectModel.Sphere.Radius, (float) ObjectModel.Sphere.Radius, (float) ObjectModel.Sphere.Radius);
                default:
                    //TODO define globaly somewhere
                    return new Vector3(0.05f, 0.01f, 0.05f);
            }
        }
    }

}
