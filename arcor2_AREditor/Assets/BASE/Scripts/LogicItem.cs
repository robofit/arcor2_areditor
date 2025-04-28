using Base;

public class LogicItem 
{
    public Arcor2.ClientSdk.Communication.OpenApi.Models.LogicItem Data;

    private Connection connection;

    public InputOutput Input {
        get;
        set;
    }

    public PuckOutput Output {
        get;
        set;
    }

    public LogicItem(Arcor2.ClientSdk.Communication.OpenApi.Models.LogicItem logicItem) {
        Data = logicItem;
        UpdateConnection(logicItem);
    }

    public void Remove() {
        Input.RemoveLogicItem(Data.Id);
        Output.RemoveLogicItem(Data.Id);
        ConnectionManagerArcoro.Instance.DestroyConnection(connection);
        connection = null;
    }

    public void UpdateConnection(Arcor2.ClientSdk.Communication.OpenApi.Models.LogicItem logicItem) {
        if (connection != null) {
            Remove();
        }
        Input = ProjectManager.Instance.GetAction(logicItem.End).Input;
        Output = ProjectManager.Instance.GetAction(logicItem.Start).Output;
        Input.AddLogicItem(Data.Id);
        Output.AddLogicItem(Data.Id);        
        connection = ConnectionManagerArcoro.Instance.CreateConnection(Input.gameObject, Output.gameObject);
        //output.Action.UpdateRotation(input.Action);
    }

    public Connection GetConnection() {
        return connection;
    }

}
