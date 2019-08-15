using System.Collections.Generic;
using UnityEngine;

public class ObjectsExporter : MonoBehaviour {

    // Start is called before the first frame update
    void Start() {

    }

    // Update is called once per frame
    void Update() {

    }

    public void ExportScene() {
        List<InteractiveObject> list = new List<InteractiveObject>();
        list.AddRange(transform.GetComponentsInChildren<InteractiveObject>());

        WebsocketManager.Instance.UpdateScene(list);
    }

    public void Export() {
        JSONObject obj = new JSONObject(JSONObject.Type.ARRAY);
        foreach (InteractiveObject io in transform.GetComponentsInChildren<InteractiveObject>()) {
            JSONObject iojson = new JSONObject(JsonUtility.ToJson(io).ToString());

            JSONObject apArray = new JSONObject(JSONObject.Type.ARRAY);
            foreach (ActionPoint ap in io.GetComponentsInChildren<ActionPoint>()) {
                JSONObject apjson = new JSONObject(JsonUtility.ToJson(ap).ToString());
                apjson.AddField("Pose", ExportRectTransform(ap.gameObject));
                JSONObject puckArray = new JSONObject(JSONObject.Type.ARRAY);
                foreach (Puck puck in ap.GetComponentsInChildren<Puck>()) {
                    JSONObject puckjson = new JSONObject(JsonUtility.ToJson(puck).ToString());
                    puckjson.AddField("Pose", ExportRectTransform(puck.gameObject));
                    if (puck.GetComponentInChildren<PuckInput>() != null && ConnectionManagerArcoro.Instance.ValidateConnection(puck.GetComponentInChildren<PuckInput>().GetConneciton())) {
                        JSONObject input_connection = new JSONObject(JSONObject.Type.STRING);
                        GameObject ConnectedPuck = ConnectionManagerArcoro.Instance.GetComponent<ConnectionManagerArcoro>().GetConnectedTo(puck.GetComponentInChildren<PuckInput>().GetConneciton(), puck.gameObject.GetComponentInChildren<PuckInput>().gameObject);
                        Debug.Log(ConnectedPuck);
                        if (ConnectedPuck != null) {
                            input_connection.str = ConnectedPuck.transform.parent.GetComponent<Puck>().id;
                            puckjson.AddField("InputConnection", input_connection);
                        }

                    }
                    if (puck.GetComponentInChildren<PuckOutput>() != null && ConnectionManagerArcoro.Instance.GetComponent<ConnectionManagerArcoro>().ValidateConnection(puck.GetComponentInChildren<PuckOutput>().GetConneciton())) {
                        JSONObject output_connection = new JSONObject(JSONObject.Type.STRING);
                        GameObject ConnectedPuck = ConnectionManagerArcoro.Instance.GetComponent<ConnectionManagerArcoro>().GetConnectedTo(puck.GetComponentInChildren<PuckOutput>().GetConneciton(), puck.gameObject.GetComponentInChildren<PuckOutput>().gameObject);
                        if (ConnectedPuck != null) {
                            output_connection.str = ConnectedPuck.transform.parent.GetComponent<Puck>().id;
                            puckjson.AddField("OutputConnection", output_connection);
                        }
                    }

                    puckArray.Add(puckjson);
                }
                apjson.AddField("Actions", puckArray);
                apArray.Add(apjson);
            }
            iojson.AddField("ActionPoints", apArray);
            obj.Add(iojson);

        }
        Debug.Log(obj.ToString(true));
    }

    public JSONObject ExportRectTransform(GameObject obj) {
        JSONObject jsonObj = new JSONObject();
        jsonObj.AddField("Position", obj.transform.position.ToString());
        jsonObj.AddField("Orientation", obj.transform.rotation.ToString());
        return jsonObj;
    }
}
