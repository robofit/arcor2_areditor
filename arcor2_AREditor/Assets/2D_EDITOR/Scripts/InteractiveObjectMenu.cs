using UnityEngine;
using UnityEngine.UI;

public class InteractiveObjectMenu : MonoBehaviour {
    public GameObject CurrentObject;
    [SerializeField]
    private GameObject aPPrefab, robotsList, endEffectorList, updatePositionButton;

    // Start is called before the first frame update
    void Start() {

    }

    // Update is called once per frame
    void Update() {

    }

    public void CreateNewAP() {
        if (CurrentObject == null) {
            return;
        }
        GameManager.Instance.SpawnActionPoint(CurrentObject.GetComponent<Base.ActionObject>());

    }

    public void SaveID(string new_id) {
        CurrentObject.GetComponent<Base.ActionObject>().Data.Id = new_id;
        GameManager.Instance.UpdateScene();
    }

    public void DeleteIO() {
        CurrentObject.GetComponent<Base.ActionObject>().DeleteIO();
        CurrentObject = null;
    }

    public void UpdateMenu() {
        Dropdown dropdown = robotsList.GetComponent<Dropdown>();
        Dropdown endEffectorDropdown = endEffectorList.GetComponent<Dropdown>();
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
            endEffectorDropdown.interactable = true;
            dropdown.interactable = true;
            updatePositionButton.GetComponent<Button>().interactable = true;
            dropdown.captionText.text = dropdown.options[dropdown.value].text;
        } else {
            endEffectorDropdown.interactable = false;
            dropdown.interactable = false;
            updatePositionButton.GetComponent<Button>().interactable = false;
        }


        endEffectorDropdown.options.Clear();
        endEffectorDropdown.captionText.text = "EE_Big";
        endEffectorDropdown.value = 0;
        endEffectorDropdown.options.Add(new Dropdown.OptionData {
            text = "EE_Big"
        });
        endEffectorDropdown.options.Add(new Dropdown.OptionData {
            text = "EE_Small"
        });
    }

    public void UpdateActionPointPosition() {
        Dropdown dropdown = robotsList.GetComponent<Dropdown>();
        Dropdown dropdownEE = endEffectorList.GetComponent<Dropdown>();
        GameManager.Instance.UpdateActionObjectPosition(CurrentObject.GetComponent<Base.ActionObject>(), dropdown.options[dropdown.value].text, dropdownEE.options[dropdownEE.value].text);
    }
}
