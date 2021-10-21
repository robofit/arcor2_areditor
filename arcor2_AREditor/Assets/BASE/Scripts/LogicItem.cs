using System;
using Base;
using UnityEngine;

public class LogicItem 
{
    public IO.Swagger.Model.LogicItem Data;

    private Connection connection;

    private InputOutput input;
    private PuckOutput output;

    public InputOutput Input {
        get => input;
        set => input = value;
    }
    public PuckOutput Output {
        get => output;
        set => output = value;
    }

    public LogicItem(IO.Swagger.Model.LogicItem logicItem) {
        Data = logicItem;
        UpdateConnection(logicItem);
    }

    public void Remove() {
        input.RemoveLogicItem(Data.Id);
        output.RemoveLogicItem(Data.Id);
        ConnectionManagerArcoro.Instance.DestroyConnection(connection);
        connection = null;
    }

    public void UpdateConnection(IO.Swagger.Model.LogicItem logicItem) {
        if (connection != null) {
            Remove();
        }
        input = ProjectManager.Instance.GetAction(logicItem.End).Input;
        output = ProjectManager.Instance.GetAction(logicItem.Start).Output;
        input.AddLogicItem(Data.Id);
        output.AddLogicItem(Data.Id);        
        connection = ConnectionManagerArcoro.Instance.CreateConnection(input.gameObject, output.gameObject);
        //output.Action.UpdateRotation(input.Action);
    }

    public Connection GetConnection() {
        return connection;
    }

}
