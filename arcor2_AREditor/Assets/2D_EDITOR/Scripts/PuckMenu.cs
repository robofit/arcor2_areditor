using System;
using UnityEngine;
using UnityEngine.UI;

public class PuckMenu : MonoBehaviour {

    public GameObject CurrentPuck;
    GameManager GameManager;

    public GameObject ParameterStringPrefab, ParameterActionPointPrefab, ParameterIntegerPrefab;
    // Start is called before the first frame update
    void Start() {
        GameManager = GameObject.Find("_GameManager").GetComponent<GameManager>();
    }

    // Update is called once per frame
    void Update() {

    }

    public void UpdateMenu(Action action, GameObject puck) {
        CurrentPuck = puck;
        foreach (RectTransform o in transform.Find("Layout").Find("DynamicContent").GetComponentsInChildren<RectTransform>()) {
            if (o.name != "Layout" && o.gameObject.tag != "Persistent") {
                Destroy(o.gameObject);
            }
        }
        transform.Find("Layout").Find("TopText").GetComponent<InputField>().text = action.Name;
        transform.Find("Layout").Find("ActionType").GetComponent<Text>().text = action.InteractiveObject.Id + "/" + action.Metadata.Name;
        foreach (ActionParameter parameter in action.Parameters.Values) {
            Debug.Log(parameter.ToString());
            GameObject paramGO = InitializeParameter(parameter);
            if (paramGO == null)
                continue;
            paramGO.transform.SetParent(transform.Find("Layout").Find("DynamicContent"));
            paramGO.transform.localScale = new Vector3(1, 1, 1);
        }
    }

    public void SaveID(string new_id) {
        CurrentPuck.GetComponent<Puck>().UpdateId(new_id);
    }

    public void DeletePuck() {
        if (CurrentPuck == null)
            return;
        CurrentPuck.GetComponent<Puck>().DeletePuck();
    }

    GameObject InitializeParameter(ActionParameter actionParameter) {
        switch (actionParameter.ActionParameterMetadata.Type) {
            case ActionParameterMetadata.Types.String:
                string value;
                try {
                    value = actionParameter.Value["value"].str;
                } catch (NullReferenceException e) {
                    Debug.Log(actionParameter.Value);
                    Debug.Log("Parse error in InitializeActionPointParameter()");
                    value = "";
                }
                return InitializeStringParameter(actionParameter.ActionParameterMetadata.Name, value);
            case ActionParameterMetadata.Types.ActionPoint:
                return InitializeActionPointParameter(actionParameter);
            case ActionParameterMetadata.Types.Integer:
                actionParameter.GetValue(out long longValue);
                return InitializeIntegerParameter(actionParameter.ActionParameterMetadata.Name, longValue);
            case ActionParameterMetadata.Types.Double:
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


    GameObject InitializeActionPointParameter(ActionParameter actionParameter) {
        GameObject actionInput = Instantiate(ParameterActionPointPrefab);
        actionInput.transform.Find("Label").GetComponent<Text>().text = actionParameter.ActionParameterMetadata.Name;
        Dropdown dropdown = actionInput.transform.Find("Dropdown").GetComponent<Dropdown>();
        dropdown.options.Clear();

        string selectedActionId;
        try {
            selectedActionId = actionParameter.Value["value"].str;
        } catch (NullReferenceException e) {
            Debug.Log(actionParameter.Value);
            Debug.Log("Parse error in InitializeActionPointParameter()");
            selectedActionId = "";
        }
        int selectedValue = -1;

        foreach (ActionPoint ap in GameManager.InteractiveObjects.GetComponentsInChildren<ActionPoint>()) {
            Dropdown.OptionData option = new Dropdown.OptionData();
            option.text = ap.IntObj.GetComponent<InteractiveObject>().Id + "." + ap.id;
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

    GameObject InitializeDoubleParameter(ActionParameter actionParameter) {
        return null;
    }

    public void OnChangeActionPointParameterHandler(string parameterId, string actionId) {
        OnChangeStringParameterHandler(parameterId, actionId);
    }

    public void OnChangeStringParameterHandler(string parameterId, string newValue) {
        if (!CurrentPuck.GetComponent<Puck>().Action.Parameters.TryGetValue(parameterId, out ActionParameter parameter))
            return;
        JSONObject value = new JSONObject(JSONObject.Type.OBJECT);
        value.AddField("value", newValue);
        parameter.Value = value;
        GameManager.UpdateProject();
    }

    public void OnChangeIntegerParameterHandler(string parameterId, long newValue) {
        if (!CurrentPuck.GetComponent<Puck>().Action.Parameters.TryGetValue(parameterId, out ActionParameter parameter))
            return;
        JSONObject value = new JSONObject(JSONObject.Type.OBJECT);
        value.AddField("value", newValue);
        parameter.Value = value;

        GameManager.UpdateProject();
    }

}
