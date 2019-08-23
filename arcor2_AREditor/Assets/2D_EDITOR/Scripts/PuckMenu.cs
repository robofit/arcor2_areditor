using System;
using UnityEngine;
using UnityEngine.UI;

public class PuckMenu : MonoBehaviour {

    public Puck2D CurrentPuck;

    public GameObject ParameterStringPrefab, ParameterActionPointPrefab, ParameterIntegerPrefab;
    // Start is called before the first frame update
    void Start() {
    }

    // Update is called once per frame
    void Update() {

    }

    public void UpdateMenu(Base.Action action, Puck2D puck) {
        CurrentPuck = puck;
        foreach (RectTransform o in transform.Find("Layout").Find("DynamicContent").GetComponentsInChildren<RectTransform>()) {
            if (o.name != "Layout" && o.gameObject.tag != "Persistent") {
                Destroy(o.gameObject);
            }
        }
        transform.Find("Layout").Find("TopText").GetComponent<InputField>().text = action.Data.Id;
        transform.Find("Layout").Find("ActionType").GetComponent<Text>().text = action.ActionObject.Data.Id + "/" + action.Metadata.Name;
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

    GameObject InitializeParameter(Base.ActionParameter actionParameter) {
        switch (actionParameter.ActionParameterMetadata.Type) {
            case Base.ActionParameterMetadata.Types.String:
                actionParameter.GetValue(out string value);       
                return InitializeStringParameter(actionParameter.ActionParameterMetadata.Name, value);
            case Base.ActionParameterMetadata.Types.ActionPoint:
                return InitializeActionPointParameter(actionParameter);
            case Base.ActionParameterMetadata.Types.Integer:
                actionParameter.GetValue(out long longValue);
                return InitializeIntegerParameter(actionParameter.ActionParameterMetadata.Name, longValue);
            case Base.ActionParameterMetadata.Types.Double:
                return InitializeDoubleParameter(actionParameter);

        }
        return null;
    }

    GameObject InitializeStringParameter(string parameterId, string value) {
        GameObject input = Instantiate(ParameterStringPrefab);
        input.transform.Find("Label").GetComponent<Text>().text = parameterId;

        input.transform.Find("Input").GetComponent<InputField>().text = value;
        input.transform.Find("Input").GetComponent<InputField>().onEndEdit.AddListener((string newValue) => OnChangeStringParameterHandler(parameterId, newValue));
        return input;
    }


    GameObject InitializeActionPointParameter(Base.ActionParameter actionParameter) {
        GameObject actionInput = Instantiate(ParameterActionPointPrefab);
        actionInput.transform.Find("Label").GetComponent<Text>().text = actionParameter.ActionParameterMetadata.Name;
        Dropdown dropdown = actionInput.transform.Find("Dropdown").GetComponent<Dropdown>();
        dropdown.options.Clear();

        actionParameter.GetValue(out string selectedActionId);

        int selectedValue = -1;

        foreach (Base.ActionPoint ap in GameManager.Instance.ActionObjects.GetComponentsInChildren<Base.ActionPoint>()) {
            Dropdown.OptionData option = new Dropdown.OptionData {
                text = ap.ActionObject.GetComponent<Base.ActionObject>().Data.Id + "." + ap.Data.Id
            };
            dropdown.options.Add(option);
            if (option.text == selectedActionId) {
                selectedValue = dropdown.options.Count - 1;
            }
        }
        if (selectedValue >= 0) {
            dropdown.value = selectedValue;
        }
        dropdown.onValueChanged.AddListener((int value) => OnChangeActionPointParameterHandler(actionParameter.ActionParameterMetadata.Name, dropdown.options[value].text));
        return actionInput;
    }

    GameObject InitializeIntegerParameter(string parameterId, long value) {
        GameObject input = Instantiate(ParameterIntegerPrefab);
        input.transform.Find("Label").GetComponent<Text>().text = parameterId;
        input.transform.Find("Input").GetComponent<InputField>().text = value.ToString();
        input.transform.Find("Input").GetComponent<InputField>().onEndEdit.AddListener((string newValue) => OnChangeIntegerParameterHandler(parameterId, long.Parse(newValue)));
        return input;
    }

    GameObject InitializeDoubleParameter(Base.ActionParameter actionParameter) {
        return null;
    }

    public void OnChangeActionPointParameterHandler(string parameterId, string actionId) {
        OnChangeStringParameterHandler(parameterId, actionId);
    }

    public void OnChangeStringParameterHandler(string parameterId, string newValue) {
        if (!CurrentPuck.Parameters.TryGetValue(parameterId, out Base.ActionParameter parameter))
            return;
        parameter.Data.Value = newValue;
        GameManager.Instance.UpdateProject();
    }

    public void OnChangeIntegerParameterHandler(string parameterId, long newValue) {
        if (!CurrentPuck.Parameters.TryGetValue(parameterId, out Base.ActionParameter parameter))
            return;
        parameter.Data.Value = newValue;
        GameManager.Instance.UpdateProject();
    }

}
