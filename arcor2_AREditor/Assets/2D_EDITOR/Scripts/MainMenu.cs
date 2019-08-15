using UnityEngine;
using UnityEngine.UI;



public class MainMenu : MonoBehaviour {
    public GameObject InteractiveObjects, ButtonPrefab;
    ActionsManager ActionsManager;
    GameManager GameManager;
    public GameObject ProjectControlButtons, ConnectionControl, ConnectionStatus, DynamicContent; //defined in inspector


    // Start is called before the first frame update
    void Start() {
        ActionsManager = GameObject.Find("_ActionsManager").GetComponent<ActionsManager>();
        GameManager = GameObject.Find("_GameManager").GetComponent<GameManager>();
    }

    // Update is called once per frame
    void Update() {

    }


    void ActionObjectsUpdated() {

        foreach (Button b in DynamicContent.GetComponentsInChildren<Button>()) {
            if (b.gameObject.tag == "PersistentButton") {
                continue;
            } else {
                Destroy(b.gameObject);
            }

        }
        foreach (string ao_name in ActionsManager.ActionObjectMetadata.Keys) {
            GameObject btnGO = Instantiate(ButtonPrefab);
            btnGO.transform.SetParent(DynamicContent.transform);
            btnGO.transform.localScale = new Vector3(1, 1, 1);
            Button btn = btnGO.GetComponent<Button>();
            btn.GetComponentInChildren<Text>().text = ao_name;
            btn.onClick.AddListener(() => GameManager.SpawnInteractiveObject(ao_name));
            btnGO.transform.SetAsFirstSibling();
        }

    }

    public void ShowProjectControlButtons() {
        ProjectControlButtons.SetActive(true);
    }

    public void ShowConnectionControl() {
        ConnectionControl.SetActive(true);
        ConnectionControl.GetComponentInChildren<Button>().interactable = true;
    }

    public void ShowConnectionStatus() {
        ConnectionStatus.SetActive(true);
    }

    public void ShowDynamicContent() {
        DynamicContent.SetActive(true);
    }

    public void HideProjectControlButtons() {
        ProjectControlButtons.SetActive(false);
    }

    public void HideConnectionControl() {
        ConnectionControl.SetActive(false);
    }

    public void HideConnectionStatus() {
        ConnectionStatus.SetActive(false);
    }

    public void HideDynamicContent() {
        DynamicContent.SetActive(false);
    }

    public void ConnectedToServer(string URI) {

        HideConnectionControl();
        ShowProjectControlButtons();
        ShowDynamicContent();
        string s = "Connected to: " + URI;
        Debug.Log(s);
        ConnectionStatus.GetComponentInChildren<Text>().text = s;
    }

    public void DisconnectedFromServer() {
        HideDynamicContent();
        HideProjectControlButtons();
        ShowConnectionControl();
        ConnectionStatus.GetComponentInChildren<Text>().text = "Not connected to server";
    }

    public string GetConnectionDomain() {
        return ConnectionControl.transform.Find("Domain").GetComponentInChildren<InputField>().text;
    }

    public int GetConnectionPort() {
        return int.Parse(ConnectionControl.transform.Find("Port").GetComponentInChildren<InputField>().text);
    }

    public void ConnectingToSever(string URI) {
        ConnectionControl.GetComponentInChildren<Button>().interactable = false;
        string s = "Connecting to server: " + URI;
        ConnectionStatus.GetComponentInChildren<Text>().text = s;
        Debug.Log(s);
    }


}
