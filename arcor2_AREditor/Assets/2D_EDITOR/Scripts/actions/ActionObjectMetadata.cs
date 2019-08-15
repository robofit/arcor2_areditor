using System;
using System.Collections.Generic;


public class ActionObjectMetadata
{

    string _type, _description, _baseObject;
    bool _actionsLoaded, _robot;
    Dictionary<string, ActionMetadata> _actionsMetadata = new Dictionary<string, ActionMetadata>();

	public ActionObjectMetadata(string type, string description, string baseObject)
	{
        Type = type;
        Description = description;
        BaseObject = baseObject;
        ActionsLoaded = false;
	}

    public string Type { get => _type; set => _type = value; }
    public string Description { get => _description; set => _description = value; }
    public bool ActionsLoaded { get => _actionsLoaded; set => _actionsLoaded = value; }
    public Dictionary<string, ActionMetadata> ActionsMetadata { get => _actionsMetadata; set => _actionsMetadata = value; }
    public bool Robot { get => _robot; set => _robot = value; }
    public string BaseObject { get => _baseObject; set => _baseObject = value; }
}
