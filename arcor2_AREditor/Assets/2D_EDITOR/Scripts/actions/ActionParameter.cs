using System;

public class ActionParameter {
    JSONObject _value;
    ActionParameterMetadata _actionParameterMetadata;


    public ActionParameter(ActionParameterMetadata actionParameterMetadata) {
        Value = actionParameterMetadata.DefaultValue;
        ActionParameterMetadata = actionParameterMetadata;
    }

    public ActionParameter(JSONObject value, ActionParameterMetadata actionParameterMetadata) {
        Value = value;
        _actionParameterMetadata = actionParameterMetadata;
    }


    public void GetValue(out string value, string def = "") {
        try {

            value = Value["value"].str;
        } catch (NullReferenceException e) {
            value = def;
        }
    }

    public void GetValue(out long value, long def = 0) {
        try {
            value = Value["value"].i;
        } catch (NullReferenceException e) {
            value = def;
        }
    }

    public void GetValue(out bool value, bool def = false) {
        try {

            value = Value["value"].b;
        } catch (NullReferenceException e) {
            value = def;
        }
    }

    public void SetValue(string value) {
        Value = new JSONObject(JSONObject.Type.OBJECT);
        Value.AddField("value", value);
    }

    public void SetValue(long value) {
        Value = new JSONObject(JSONObject.Type.OBJECT);
        Value.AddField("value", value);
    }

    public void SetValue(bool value) {
        Value = new JSONObject(JSONObject.Type.OBJECT);
        Value.AddField("value", value);
    }

    public ActionParameterMetadata ActionParameterMetadata {
        get => _actionParameterMetadata; set => _actionParameterMetadata = value;
    }
    public JSONObject Value {
        get => _value; set => _value = value;
    }
}
