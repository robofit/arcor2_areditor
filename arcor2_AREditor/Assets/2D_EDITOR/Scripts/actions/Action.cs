using System.Collections.Generic;

public class Action {
    string _name;
    ActionMetadata _metadata;
    InteractiveObject _interactiveObject;

    Dictionary<string, ActionParameter> _parameters = new Dictionary<string, ActionParameter>();


    public Action(string name, ActionMetadata metadata, InteractiveObject originalIO, ActionPoint ap) {
        _name = name;
        _metadata = metadata;
        _interactiveObject = originalIO;
        foreach (ActionParameterMetadata actionParameterMetadata in _metadata.Parameters.Values) {
            ActionParameter actionParameter = new ActionParameter(actionParameterMetadata);
            if (actionParameter.ActionParameterMetadata.Type == ActionParameterMetadata.Types.ActionPoint) {
                JSONObject value = new JSONObject(JSONObject.Type.OBJECT);
                value.AddField("value", ap.IntObj.GetComponent<InteractiveObject>().Id + "." + ap.id);
                actionParameter.Value = value;
            } else {
                actionParameter.Value = actionParameter.ActionParameterMetadata.DefaultValue;
            }
            Parameters[actionParameter.ActionParameterMetadata.Name] = actionParameter;
        }
    }

    public Action(string name, ActionMetadata metadata, InteractiveObject originalIO, ActionPoint ap, Dictionary<string, ActionParameter> parameters) {
        _name = name;
        _metadata = metadata;
        Parameters = parameters;
        _interactiveObject = originalIO;
    }

    public string Name {
        get => _name; set => _name = value;
    }
    public Dictionary<string, ActionParameter> Parameters {
        get => _parameters; set => _parameters = value;
    }
    public ActionMetadata Metadata {
        get => _metadata; set => _metadata = value;
    }
    public InteractiveObject InteractiveObject {
        get => _interactiveObject; set => _interactiveObject = value;
    }
}
