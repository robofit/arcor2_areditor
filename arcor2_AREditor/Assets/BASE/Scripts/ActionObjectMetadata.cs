using System.Collections.Generic;
using Arcor2.ClientSdk.Communication.OpenApi.Models;
using UnityEngine;

namespace Base {
    public class ActionObjectMetadata : ObjectTypeMeta {
        public ActionObjectMetadata(ObjectTypeMeta meta) : base(varAbstract: meta.Abstract,
                                                                varBase: meta.Base,
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
            get;
            set;
        }

        public bool Camera {
            get;
            set;
        }

        public bool ActionsLoaded {
            get;
            set;
        }

        public Dictionary<string, ActionMetadata> ActionsMetadata {
            get;
            set;
        } = new();

        public bool CollisionObject {
            get;
            set;
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
