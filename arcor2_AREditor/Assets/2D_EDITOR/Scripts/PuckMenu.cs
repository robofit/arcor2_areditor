using System;
using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;

public class PuckMenu : MonoBehaviour {

    public Base.Action CurrentPuck;

    public GameObject ParameterStringPrefab, ParameterActionPointPrefab, ParameterIntegerPrefab, ParameterDoublePrefab;
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
            Debug.Log(parameter.ToString());
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
        switch (actionParameter.ActionParameterMetadata.Type) {
            case IO.Swagger.Model.ObjectActionArgs.TypeEnum.String:
                actionParameter.GetValue(out string value);       
                return InitializeStringParameter(actionParameter.ActionParameterMetadata.Name, value);
           case IO.Swagger.Model.ObjectActionArgs.TypeEnum.Pose:
                return InitializePoseParameter(actionParameter);
            case IO.Swagger.Model.ObjectActionArgs.TypeEnum.Integer:
                actionParameter.GetValue(out long longValue);
                return InitializeIntegerParameter(actionParameter.ActionParameterMetadata.Name, longValue);
            case IO.Swagger.Model.ObjectActionArgs.TypeEnum.Double:
                return InitializeDoubleParameter(actionParameter);

        }
        return null;
    }

    private GameObject InitializeStringParameter(string parameterId, string value) {
        GameObject input = Instantiate(ParameterStringPrefab);
        input.GetComponent<LabeledInput>().Label.text = parameterId;
        input.GetComponent<LabeledInput>().Input.text = value;
        input.GetComponent<LabeledInput>().Input.onEndEdit.AddListener((string newValue)
            => OnChangeParameterHandler(parameterId, newValue));
        return input;
    }

    private GameObject InitializePoseParameter(Base.ActionParameter actionParameter) {
        GameObject actionInput = Instantiate(ParameterActionPointPrefab);
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
        return actionInput;
    }

    private GameObject InitializeIntegerParameter(string parameterId, long value) {
        GameObject input = Instantiate(ParameterIntegerPrefab);
        input.GetComponent<LabeledInput>().Label.text = parameterId;
        input.GetComponent<LabeledInput>().Input.text = value.ToString();
        input.GetComponent<LabeledInput>().Input.onEndEdit.AddListener((string newValue)
            => OnChangeParameterHandler(parameterId, long.Parse(newValue)));
        return input;
    }

    private GameObject InitializeDoubleParameter(Base.ActionParameter actionParameter) {
        GameObject input = Instantiate(ParameterDoublePrefab);
        input.GetComponent<LabeledInput>().Label.text = actionParameter.ActionParameterMetadata.Name;
        actionParameter.GetValue(out double value);
        input.GetComponent<LabeledInput>().Input.text = value.ToString();
        input.GetComponent<LabeledInput>().Input.onEndEdit.AddListener((string newValue)
            => OnChangeParameterHandler(actionParameter.ActionParameterMetadata.Name, double.Parse(newValue)));
        return input;
    }

   public void OnChangeParameterHandler(string parameterId, object newValue) {
        if (!CurrentPuck.Parameters.TryGetValue(parameterId, out Base.ActionParameter parameter))
            return;
        parameter.Value = newValue;
        Base.GameManager.Instance.UpdateProject();
    }

}
