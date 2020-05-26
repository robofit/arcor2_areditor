using System;
using System.Collections.Generic;

public class Robot : IRobot
{
    private readonly string id;
    private readonly string name;
    private readonly List<string> endEffectors;
	public Robot(string id, string name, List<string> endEffectors)
	{
        this.id = id;
        this.name = name;
        this.endEffectors = endEffectors;
	}

    public List<string> GetEndEffectors() {
        return endEffectors;
    }

    public string GetId() {
        return id;
    }

    public string GetName() {
        return name;
    }
}
