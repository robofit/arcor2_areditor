using System;
using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;
using System.Globalization;
using System.Threading.Tasks;
using UnityEngine.Events;
using Michsky.UI.ModernUIPack;
using Newtonsoft.Json;

public class ActionMenu : Base.Singleton<ActionMenu>, IMenu {

    public Base.Action CurrentPuck;

    public GameObject ParameterInputPrefab, ParameterDropdownPrefab;
    public GameObject DynamicContent;
    public InputField TopText;
    public Text ActionType;
    public Button ExectuteActionBtn;
    // Start is called before the first frame update


    public async void UpdateMenu(Base.Action action) {
        DynamicContent.GetComponent<VerticalLayoutGroup>().enabled = true;
        CurrentPuck = action;
        foreach (RectTransform o in DynamicContent.GetComponentsInChildren<RectTransform>()) {
            if (o.name != "Layout" && o.gameObject.tag != "Persistent") {
                Destroy(o.gameObject);
            }
        }
        SetHeader(action.Data.Id);
        ActionType.text = action.Data.Type;
        List<Tuple<DropdownParameter, Base.ActionParameter>> dynamicDropdowns = new List<Tuple<DropdownParameter, Base.ActionParameter>>();
        foreach (Base.ActionParameter parameter in action.Parameters.Values) {
            GameObject paramGO = await InitializeParameter(parameter);
            if (paramGO == null)
                continue;
            if (parameter.ActionParameterMetadata.DynamicValue) {
                dynamicDropdowns.Add(new Tuple<DropdownParameter, Base.ActionParameter>(paramGO.GetComponent<DropdownParameter>(), parameter));
            }
            paramGO.transform.SetParent(DynamicContent.transform);
            paramGO.transform.localScale = new Vector3(1, 1, 1);
        }
        int parentCount = 0;
        while (dynamicDropdowns.Count > 0) {
            for (int i = dynamicDropdowns.Count - 1; i >= 0; i--) {
                Tuple<DropdownParameter, Base.ActionParameter> tuple = dynamicDropdowns[i];
                if (tuple.Item2.ActionParameterMetadata.DynamicValueParents.Count == parentCount) {
                    try {
                        await LoadDropdownValues(tuple.Item1, tuple.Item2, async () => await LoadDropdownValues(tuple.Item1, tuple.Item2));
                    } catch (Exception ex) when (ex is Base.ItemNotFoundException || ex is Base.RequestFailedException) {
                        Debug.LogError(ex);
                    } finally {
                        dynamicDropdowns.RemoveAt(i);
                    }
                }
            }
            parentCount += 1;
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

    private async Task<GameObject> InitializeParameter(Base.ActionParameter actionParameter) {
        GameObject parameter = null;
        switch (actionParameter.ActionParameterMetadata.Type) {
            case "string":
                 parameter = InitializeStringParameter(actionParameter);
                break;
            case "relative_pose":
                parameter = InitializeRelativePoseParameter(actionParameter);
                break;
            case "pose":
                parameter = InitializePoseParameter(actionParameter);
                break;
            case "joints":
                parameter = InitializeJointsParameter(actionParameter);
                break;
            case "string_enum":
                parameter = InitializeStringEnumParameter(actionParameter);
                break;
            case "integer_enum":
                parameter = InitializeIntegerEnumParameter(actionParameter);
                break;
            case "integer":
                parameter = InitializeIntegerParameter(actionParameter);
                break;
            case "double":
                parameter = InitializeDoubleParameter(actionParameter);
                break;

        }
        if (parameter == null) {
            return null;
        } else {
            parameter.GetComponent<IActionParameter>().SetLabel(actionParameter.Id, actionParameter.ActionParameterMetadata.Description);
            return parameter;
        }

    }

    private GameObject InitializeStringParameter(Base.ActionParameter actionParameter) {
        GameObject input;
        if (actionParameter.ActionParameterMetadata.DynamicValue) {
            string selectedValue = actionParameter.GetValue<string>();
            input = InitializeDropdownParameter(actionParameter, new List<string>(), selectedValue);
            input.GetComponent<DropdownParameter>().SetLoading(true);
        } else {
            string value =  actionParameter.GetValue<string>();
            input = Instantiate(ParameterInputPrefab);
            input.GetComponent<LabeledInput>().SetType(TMPro.TMP_InputField.ContentType.Standard);
            input.GetComponent<LabeledInput>().SetValue(value);
            input.GetComponent<LabeledInput>().Input.onEndEdit.AddListener((string newValue)
                => OnChangeParameterHandler(actionParameter.Id, newValue));
        }
        return input;
    }

    private GameObject InitializeRelativePoseParameter(Base.ActionParameter actionParameter) {
        GameObject input;
        
        IO.Swagger.Model.Pose value = actionParameter.GetValue<IO.Swagger.Model.Pose>();
        input = Instantiate(ParameterInputPrefab);
        input.GetComponent<LabeledInput>().SetType(TMPro.TMP_InputField.ContentType.Standard);
        input.GetComponent<LabeledInput>().SetValue(JsonConvert.SerializeObject(value));
        input.GetComponent<LabeledInput>().Input.onEndEdit.AddListener((string newValue)
            => OnChangeParameterHandler(actionParameter.Id, JsonConvert.DeserializeObject<IO.Swagger.Model.Pose>(newValue)));
        
        return input;
    }

    private async Task LoadDropdownValues(DropdownParameter dropdownParameter, Base.ActionParameter actionParameter, UnityAction callback = null) {
        List<string> values = new List<string>();
        List<IO.Swagger.Model.IdValue> args = new List<IO.Swagger.Model.IdValue>();
        foreach (string parent_param in actionParameter.ActionParameterMetadata.DynamicValueParents) {
            IO.Swagger.Model.IdValue idValue = new IO.Swagger.Model.IdValue(id: parent_param, value: GetDropdownParamValue(parent_param));
            args.Add(idValue);
            if (callback != null)
                AddOnChangeToDropdownParameter(parent_param, callback);
        }
        values = await actionParameter.LoadDynamicValues(args);
        DropdownParameterPutData(dropdownParameter, values, actionParameter.GetValue<string>(), actionParameter.ActionParameterMetadata.Name);
    }

    private string GetDropdownParamValue(string param_id) {
        if (GetDropdownParameter(param_id).Dropdown.dropdownItems.Count == 0)
            return "";
        return GetDropdownParameter(param_id).Dropdown.selectedText.text;
    }

    private DropdownParameter GetDropdownParameter(string param_id) {
        foreach (DropdownParameter dropdownParameter in DynamicContent.GetComponentsInChildren<DropdownParameter>()) {
            if (dropdownParameter.Label.text == param_id)
                return dropdownParameter;
        }
        throw new Base.ItemNotFoundException("Parameter not found: " + param_id);
    }

    private void AddOnChangeToDropdownParameter(string param_id, UnityAction callback) {
        DropdownParameter dropdownParameter = GetDropdownParameter(param_id);
        foreach (CustomDropdown.Item item in dropdownParameter.Dropdown.dropdownItems) {
            item.OnItemSelection.AddListener(callback);
        }
    }

    private GameObject InitializeDropdownParameter(Base.ActionParameter actionParameter, List<string> data, string selectedValue) {
        GameObject dropdownParameter = Instantiate(ParameterDropdownPrefab, DynamicContent.transform);
        dropdownParameter.GetComponent<DropdownParameter>().Init();
        DropdownParameterPutData(dropdownParameter.GetComponent<DropdownParameter>(), data, selectedValue, actionParameter.ActionParameterMetadata.Name);
        return dropdownParameter;
    }

    private void DropdownParameterPutData(DropdownParameter dropdownParameter, List<string> data, string selectedValue, string parameterId) {
        dropdownParameter.PutData(data, selectedValue,
            () => OnChangeParameterHandler(parameterId, dropdownParameter.Dropdown.selectedText.text));
        if (selectedValue == "" || selectedValue == null) {
            string value;
            if (dropdownParameter.Dropdown.dropdownItems.Count == 0)
                value = "";
            else
                value = dropdownParameter.Dropdown.selectedText.text;

            OnChangeParameterHandler(parameterId, value);
        }
    }

    private GameObject InitializeStringEnumParameter(Base.ActionParameter actionParameter) {
        string selectedValue = actionParameter.GetValue<string>();
        List<string> data = new List<string>();
        foreach (string item in ((ARServer.Models.StringEnumParameterExtra) actionParameter.ActionParameterMetadata.ParameterExtra).AllowedValues)
            data.Add(item);
        return InitializeDropdownParameter(actionParameter, data, selectedValue);
    }

    private GameObject InitializeIntegerEnumParameter(Base.ActionParameter actionParameter) {
        List<string> options = new List<string>();
        foreach (int item in ((ARServer.Models.IntegerEnumParameterExtra) actionParameter.ActionParameterMetadata.ParameterExtra).AllowedValues) {
            options.Add(item.ToString());
        }
        int selectedValue = actionParameter.GetValue<int>();

        return InitializeDropdownParameter(actionParameter, options, selectedValue.ToString());
    }

    private GameObject InitializePoseParameter(Base.ActionParameter actionParameter) {
        List<string> options = new List<string>();
        foreach (Base.ActionPoint ap in Base.Scene.Instance.GetAllActionPoints()) {
            foreach (string poseKey in ap.GetPoses().Keys) {
                options.Add(ap.ActionObject.GetComponent<Base.ActionObject>().Data.Id + "." + ap.Data.Id + "." + poseKey);
            }
        }
        
        string selectedValue = actionParameter.GetValue<string>();

        return InitializeDropdownParameter(actionParameter, options, selectedValue);
    }

    private GameObject InitializeJointsParameter(Base.ActionParameter actionParameter) {
        List<string> options = new List<string>();
        foreach (Base.ActionPoint ap in Base.Scene.Instance.GetAllActionPoints()) {
            foreach (string jointsId in ap.GetJoints().Keys) {
                options.Add(ap.ActionObject.GetComponent<Base.ActionObject>().Data.Id + "." + ap.Data.Id + "." + jointsId);
            }
        }
        string selectedValue = actionParameter.GetValue<string>();
        return InitializeDropdownParameter(actionParameter, options, selectedValue);
    }

    private GameObject InitializeIntegerParameter(Base.ActionParameter actionParameter) {
        GameObject input = Instantiate(ParameterInputPrefab);
        int value = actionParameter.GetValue<int>();
        input.GetComponent<LabeledInput>().SetType(TMPro.TMP_InputField.ContentType.IntegerNumber);
        input.GetComponent<LabeledInput>().Input.text = value.ToString();
        input.GetComponent<LabeledInput>().Input.onEndEdit.AddListener((string newValue)
            => OnChangeParameterHandler(actionParameter.Id, long.Parse(newValue)));
        return input;
    }

    private GameObject InitializeDoubleParameter(Base.ActionParameter actionParameter) {
        GameObject input = Instantiate(ParameterInputPrefab);
        input.GetComponent<LabeledInput>().SetType(TMPro.TMP_InputField.ContentType.DecimalNumber);
        double value = actionParameter.GetValue<double>();
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
        parameter.SetValue(newValue);
        Base.GameManager.Instance.UpdateProject();
    }

    public async void ExecuteAction() {
        ExectuteActionBtn.interactable = false;
        if (await Base.GameManager.Instance.ExecuteAction(CurrentPuck.Data.Id)) {

        }
        ExectuteActionBtn.interactable = true;
     
    }

    public void UpdateMenu() {
        UpdateMenu(CurrentPuck);
    }

    public void SetHeader(string header) {
        TopText.text = header;
    }
}
