using System;
using Base;
using UnityEngine;

public class LogicItem 
{
    public IO.Swagger.Model.LogicItem Data;

    private Connection connection;

    private PuckInput input;
    private PuckOutput output;

    public LogicItem(IO.Swagger.Model.LogicItem logicItem) {
        Data = logicItem;
        UpdateConnection(logicItem);
    }

    public void Remove() {
        input.Init(null);
        output.Init(null);
        UnityEngine.Object.Destroy(connection.gameObject);
        connection = null;
    }

    public void UpdateConnection(IO.Swagger.Model.LogicItem logicItem) {
        if (connection != null) {
            Remove();
        }
        input = ProjectManager.Instance.GetAction(logicItem.End).Input;
        output = ProjectManager.Instance.GetAction(logicItem.Start).Output;
        input.Init(Data.Id);
        output.Init(Data.Id);        
        connection = ConnectionManagerArcoro.Instance.CreateConnection(input.gameObject, output.gameObject);
    }

    public Connection GetConnection() {
        return connection;
    }

}
