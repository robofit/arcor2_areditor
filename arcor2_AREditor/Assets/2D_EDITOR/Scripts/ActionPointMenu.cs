using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ActionPointMenu : MonoBehaviour {
    [System.NonSerialized]
    public Base.ActionPoint CurrentActionPoint;
    public GameObject ActionButtonPrefab;

    [SerializeField]
    private GameObject dynamicContent, topText, interactiveObjectType, robotsList, updatePositionButton, endEffectorList;

    // Start is called before the first frame update
    private void Start() {

    }

    // Update is called once per frame
    private void Update() {

    }

    public void CreatePuck(string action_id, Base.ActionObject actionObject) {
        Debug.LogWarning(action_id);
        GameManager.Instance.SpawnPuck(action_id, CurrentActionPoint, actionObject);
    }

    public void SaveID(string new_id) {
        CurrentActionPoint.GetComponent<Base.ActionPoint>().Data.Id = new_id;
    }

    public void UpdateMenu() {

        Base.ActionPoint actionPoint;
        if (CurrentActionPoint == null) {
            return;
        } else {
            actionPoint = CurrentActionPoint.GetComponent<Base.ActionPoint>();
        }

        foreach (RectTransform o in dynamicContent.GetComponentsInChildren<RectTransform>()) {
            if (o.gameObject.tag != "Persistent") {
                Destroy(o.gameObject);
            }
        }
        topText.GetComponentInChildren<Text>().text = actionPoint.Data.Id;
        interactiveObjectType.GetComponent<Text>().text = actionPoint.ActionObject.GetComponent<Base.ActionObject>().Data.Type;

        foreach (KeyValuePair<Base.ActionObject, List<Base.ActionMetadata>> keyval in ActionsManager.Instance.GetAllActionsOfObject(actionPoint.ActionObject.GetComponent<Base.ActionObject>())) {
            foreach (Base.ActionMetadata am in keyval.Value) {
                GameObject btnGO = Instantiate(GameManager.Instance.ButtonPrefab);
                btnGO.transform.SetParent(dynamicContent.transform);
                btnGO.transform.localScale = new Vector3(1, 1, 1);
                Button btn = btnGO.GetComponent<Button>();
                btn.GetComponentInChildren<Text>().text = keyval.Key.Data.Id + "/" + am.Name;
                btn.onClick.AddListener(() => CreatePuck(am.Name, keyval.Key));
            }

        }
        Dropdown dropdown = robotsList.GetComponent<Dropdown>();
        dropdown.options.Clear();
        dropdown.captionText.text = "";
        foreach (Base.ActionObject actionObject in GameManager.Instance.ActionObjects.GetComponentsInChildren<Base.ActionObject>()) {
            if (actionObject.ActionObjectMetadata.Robot) {
                Dropdown.OptionData option = new Dropdown.OptionData {
                    text = actionObject.Data.Id
                };
                dropdown.options.Add(option);
            }
        }
        dropdown.value = 0;
        if (dropdown.options.Count > 0) {
            dropdown.interactable = true;
            updatePositionButton.GetComponent<Button>().interactable = true;
            dropdown.captionText.text = dropdown.options[dropdown.value].text;
        } else {
            dropdown.interactable = false;
            updatePositionButton.GetComponent<Button>().interactable = false;
        }
    }

    public void DeleteAP() {
        if (CurrentActionPoint == null)
            return;
        CurrentActionPoint.GetComponent<Base.ActionPoint>().DeleteAP();
    }

    public void UpdateActionPointPosition() {
        Dropdown dropdown = robotsList.GetComponent<Dropdown>();
        GameManager.Instance.UpdateActionPointPosition(CurrentActionPoint.GetComponent<Base.ActionPoint>(), dropdown.options[dropdown.value].text);
    }

    public void UpdateEndEffectorList(Base.ActionObject robot) {

    }
}
