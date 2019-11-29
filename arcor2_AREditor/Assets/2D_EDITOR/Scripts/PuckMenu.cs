using System;
using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;
using System.Globalization;

public class PuckMenu : MonoBehaviour {

    public Base.Action CurrentPuck;

    public GameObject ParameterInputPrefab;
    // Start is called before the first frame update
   

    public void UpdateMenu(Base.Action action) {
        CurrentPuck = action;
        foreach (RectTransform o in transform.Find("Layout").Find("DynamicContent").GetComponentsInChildren<RectTransform>()) {
            if (o.name != "Layout" && o.gameObject.tag != "Persistent") {
                Destroy(o.gameObject);
            }
        }
        transform.Find("Layout").Find("TopText").GetComponent<InputField>().text = action.Data.Id;
        transform.Find("Layout").Find("ActionType").GetComponent<Text>().text = action.Data.Type;
        foreach (Base.ActionParameter parameter in action.Parameters.Values) {
            GameObject paramGO = InitializeParameter(parameter);
            if (paramGO == null)
                continue;
            paramGO.transform.SetParent(transform.Find("Layout").Find("DynamicContent"));
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
                parameter = InitializeStringParameter(actionParameter);
                break;
           case IO.Swagger.Model.ObjectActionArg.TypeEnum.Pose:
                parameter = InitializePoseParameter(actionParameter);
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

    private GameObject InitializePoseParameter(Base.ActionParameter actionParameter) {
        /*GameObject actionInput = Instantiate(ParameterActionPointPrefab);
        actionInput.transform.Find("Label").GetComponent<Text>().text = actionParameter.ActionParameterMetadata.Name;
        Dropdown dropdown = actionInput.transform.Find("Dropdown").GetComponent<Dropdown>();
        dropdown.options.Clear();

        actionParameter.GetValue(out string selectedActionId);

        int selectedValue = -1;

        foreach (Base.ActionPoint ap in Base.GameManager.Instance.ActionObjects.GetComponentsInChildren<Base.ActionPoint>()) {
            foreach (string poseKey in ap.GetPoses().Keys) {
                Dropdown.OptionData option = new Dropdown.OptionData {
                    text = ap.ActionObject.GetComponent<Base.ActionObject>().Data.Id + "." + ap.Data.Id + "." + poseKey
                };
                dropdown.options.Add(option);
                if (option.text == selectedActionId) {
                    selectedValue = dropdown.options.Count - 1;
                }
            }                     
        }
        if (selectedValue >= 0) {
            dropdown.value = selectedValue;
        }
        dropdown.onValueChanged.AddListener((int value)
            => OnChangeParameterHandler(actionParameter.ActionParameterMetadata.Name, dropdown.options[value].text));
        return actionInput;*/
        return null;
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
