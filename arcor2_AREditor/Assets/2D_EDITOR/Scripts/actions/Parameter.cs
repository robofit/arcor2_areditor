public class Parameter {
    string _parameter_id, _description;
    public Parameter(JSONObject obj) {
        _parameter_id = obj.keys[0];
        //_description = obj["description"].ToString();
    }
}
