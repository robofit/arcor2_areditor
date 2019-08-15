using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ActionPuck
{
    public string _action_id;
    List<Parameter> _parameters = new List<Parameter>();
    public ActionPuck(JSONObject obj)
    {
        
        _action_id = obj["action_id"].str;
        foreach (JSONObject parameter in obj["parameters"].list)
        {
            _parameters.Add(new Parameter(parameter));
        }
    }
}