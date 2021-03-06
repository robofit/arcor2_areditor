using System;
using System.Collections.Generic;
using System.Linq;
using Base;
using UnityEngine;
using UnityEngine.UI;

public class LinkableDropdownPoses : LinkableDropdown
{
    

    public override void Init(ParameterMetadata parameterMetadata, string type, object value, VerticalLayoutGroup layoutGroupToBeDisabled, GameObject canvasRoot, Parameter.OnChangeParameterHandlerDelegate onChangeParameterHandler, bool linkable = true) {
        base.Init(parameterMetadata, type, value, layoutGroupToBeDisabled, canvasRoot, onChangeParameterHandler, linkable);
        List<string> options = new List<string>();

        foreach (Base.ActionPoint ap in Base.ProjectManager.Instance.GetAllActionPoints()) {
            foreach (IO.Swagger.Model.NamedOrientation orientation in ap.GetNamedOrientations()) {
                options.Add(ap.Data.Name + "." + orientation.Name);
            }
        }
        string selectedValue = null;
        if (value != null) {
            try {
                ActionPoint actionPoint = ProjectManager.Instance.GetActionPointWithOrientation((string) value);
                IO.Swagger.Model.NamedOrientation namedOrientation = actionPoint.GetNamedOrientation((string) value);
                selectedValue = actionPoint.Data.Name + "." + namedOrientation.Name;
            } catch (KeyNotFoundException ex) {
                Debug.LogError(ex);
            }

        }
        if (type == "link")
            ActionsDropdown.SetValue(selectedValue);

        DropdownParameter.PutData(options, type == "link" ? null : selectedValue, (v) => onChangeParameterHandler(parameterMetadata.Name, v, type));
    }

    public override object GetValue() {
        object v = base.GetValue();
        if (type == "link")
            return v;
        else {
            string value = (string) v;
            if (value == null)
                return null;

            Base.ActionPoint actionPoint = Base.ProjectManager.Instance.GetactionpointByName(value.Split('.').First());
            return actionPoint.GetNamedOrientationByName(value.Split('.').Last()).Id;
        }    
    }
}
