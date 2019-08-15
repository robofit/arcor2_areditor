using System;
using System.Collections.Generic;


public class ActionObject
{

    string _type, _description;
    bool _actionsLoaded;
    Dictionary<string, Action> _actions = new Dictionary<string, Action>();

	public ActionObject(string type, string description)
	{
        Type = type;
        Description = description;
        ActionsLoaded = false;
	}

    public string Type { get => _type; set => _type = value; }
    public string Description { get => _description; set => _description = value; }
    public bool ActionsLoaded { get => _actionsLoaded; set => _actionsLoaded = value; }
    public Dictionary<string, Action> Actions { get => _actions; set => _actions = value; }
}
