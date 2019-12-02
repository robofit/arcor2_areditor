using System;
using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;
using System.Globalization;

public class PuckMenu : Base.Singleton<PuckMenu> {

    public Base.Action CurrentPuck;

    public GameObject ParameterInputPrefab, ParameterDropdownPrefab;
    public GameObject DynamicContent;
    public InputField TopText;
    public Text ActionType;
    // Start is called before the first frame update
   

    public void UpdateMenu(Base.Action action) {
        DynamicContent.GetComponent<VerticalLayoutGroup>().enabled = true;
        CurrentPuck = action;
        foreach (RectTransform o in DynamicContent.GetComponentsInChildren<RectTransform>()) {
            if (o.name != "Layout" && o.gameObject.tag != "Persistent") {
                Destroy(o.gameObject);
            }
        }
        TopText.text = action.Data.Id;
        ActionType.text = action.Data.Type;
        foreach (Base.ActionParameter parameter in action.Parameters.Values) {
            GameObject paramGO = InitializeParameter(parameter);
            if (paramGO == null)
                continue;
            paramGO.transform.SetParent(DynamicContent.transform);
            paramGO.transform.localScale = new Vector3(1, 1, 1);
        }
        

    }

    public void SaveID(string new_id) {
        CurrentPuck.UpdateId(new_id);
    }

    public void DeletePuck() {
        if (CurrentPuck == null)
            return;
        CurrentPuck.DeleteAction();
    }

    private GameObject InitializeParameter(Base.ActionParameter actionParameter) {
        GameObject parameter = null;
        switch (actionParameter.ActionParameterMetadata.Type) {
            case IO.Swagger.Model.ObjectActionArg.TypeEnum.String:
            case IO.Swagger.Model.ObjectActionArg.TypeEnum.Relativepose:
                parameter = InitializeStringParameter(actionParameter);
                break;
            case IO.Swagger.Model.ObjectActionArg.TypeEnum.Pose:
                parameter = InitializePoseParameter(actionParameter);
                break;
            case IO.Swagger.Model.ObjectActionArg.TypeEnum.Joints:
                parameter = InitializeJointsParameter(actionParameter);
                break;
            case IO.Swagger.Model.ObjectActionArg.TypeEnum.Stringenum:
                parameter = InitializeStringEnumParameter(actionParameter);
                break;
            case IO.Swagger.Model.ObjectActionArg.TypeEnum.Integerenum:
                parameter = InitializeIntegerEnumParameter(actionParameter);
                break;
            case IO.Swagger.Model.ObjectActionArg.TypeEnum.Integer:
                parameter = InitializeIntegerParameter(actionParameter);
                break;
            case IO.Swagger.Model.ObjectActionArg.TypeEnum.Double:
                parameter = InitializeDoubleParameter(actionParameter);
                break;

        }
        if (parameter == null) {
            return null;
        } else {
            parameter.GetComponent<IActionParameter>().SetLabel(actionParameter.Id);
            return parameter;
            ;
        }
        
    }

    private GameObject InitializeStringParameter(Base.ActionParameter actionParameter) {
        GameObject input = Instantiate(ParameterInputPrefab);
        input.GetComponent<LabeledInput>().SetType(TMPro.TMP_InputField.ContentType.Standard);
        actionParameter.GetValue(out string value);
        input.GetComponent<LabeledInput>().SetValue(value);
        input.GetComponent<LabeledInput>().Input.onEndEdit.AddListener((string newValue)
            => OnChangeParameterHandler(actionParameter.Id, newValue));
        return input;
    }



    private GameObject InitializeDropdownParameter(Base.ActionParameter actionParameter, List<string> data) {
        GameObject dropdownParameter = Instantiate(ParameterDropdownPrefab, DynamicContent.transform);
        dropdownParameter.GetComponent<DropdownParameter>().Init();
        actionParameter.GetValue(out string selectedActionId);
        
        dropdownParameter.GetComponent<DropdownParameter>().PutData(data, selectedActionId,
            () => OnChangeParameterHandler(actionParameter.ActionParameterMetadata.Name,
                                            dropdownParameter.GetComponent<DropdownParameter>().Dropdown.selectedText.text));
        return dropdownParameter;
    }

    private GameObject InitializeStringEnumParameter(Base.ActionParameter actionParameter) {
        return InitializeDropdownParameter(actionParameter, actionParameter.ActionParameterMetadata.StringAllowedValues);
    }

    private GameObject InitializeIntegerEnumParameter(Base.ActionParameter actionParameter) {
        List<string> options = new List<string>();
        foreach (int item in actionParameter.ActionParameterMetadata.IntegerAllowedValues) {
            options.Add(item.ToString());
        }
        return InitializeDropdownParameter(actionParameter, options);
    }

    private GameObject InitializePoseParameter(Base.ActionParameter actionParameter) {        
        List<string> options = new List<string>();
        foreach (Base.ActionPoint ap in Base.GameManager.Instance.ActionObjects.GetComponentsInChildren<Base.ActionPoint>()) {
            foreach (string poseKey in ap.GetPoses().Keys) {
                options.Add(ap.ActionObject.GetComponent<Base.ActionObject>().Data.Id + "." + ap.Data.Id + "." + poseKey);               
            }
        }
        return InitializeDropdownParameter(actionParameter, options);
    }

     private GameObject InitializeJointsParameter(Base.ActionParameter actionParameter) {        
        List<string> options = new List<string>();
        foreach (Base.ActionPoint ap in Base.GameManager.Instance.ActionObjects.GetComponentsInChildren<Base.ActionPoint>()) {
            foreach (string jointsId in ap.GetJoints().Keys) {
                options.Add(ap.ActionObject.GetComponent<Base.ActionObject>().Data.Id + "." + ap.Data.Id + "." + jointsId);               
            }
        }
        return InitializeDropdownParameter(actionParameter, options);
    }

    private GameObject InitializeIntegerParameter(Base.ActionParameter actionParameter) {
        GameObject input = Instantiate(ParameterInputPrefab);
        actionParameter.GetValue(out long value);
        input.GetComponent<LabeledInput>().SetType(TMPro.TMP_InputField.ContentType.IntegerNumber);
        input.GetComponent<LabeledInput>().Input.text = value.ToString();
        input.GetComponent<LabeledInput>().Input.onEndEdit.AddListener((string newValue)
            => OnChangeParameterHandler(actionParameter.Id, long.Parse(newValue)));
        return input;
    }

    private GameObject InitializeDoubleParameter(Base.ActionParameter actionParameter) {
        GameObject input = Instantiate(ParameterInputPrefab);
        input.GetComponent<LabeledInput>().SetType(TMPro.TMP_InputField.ContentType.DecimalNumber);
        actionParameter.GetValue(out double value);
        input.GetComponent<LabeledInput>().Input.text = value.ToString();
        input.GetComponent<LabeledInput>().Input.onEndEdit.AddListener((string newValue)
            => OnChangeParameterHandler(actionParameter.ActionParameterMetadata.Name, ParseDouble(newValue)));
        return input;
    }

    private double ParseDouble(string value) {
        //Try parsing in the current culture
        if (!double.TryParse(value, System.Globalization.NumberStyles.Any, CultureInfo.CurrentCulture, out double result) &&
            //Then try in US english
            !double.TryParse(value, System.Globalization.NumberStyles.Any, CultureInfo.GetCultureInfo("en-US"), out result) &&
            //Then in neutral language
            !double.TryParse(value, System.Globalization.NumberStyles.Any, CultureInfo.InvariantCulture, out result)) {

        }
        return result;
    }

   public void OnChangeParameterHandler(string parameterId, object newValue) {
        if (!CurrentPuck.Parameters.TryGetValue(parameterId, out Base.ActionParameter parameter))
            return;
        parameter.Value = newValue;
        Base.GameManager.Instance.UpdateProject();
    }

}
