using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ActionPointMenu : MonoBehaviour
{
    public GameObject CurrentActionPoint;
    ActionsManager ActionsManager;
    GameManager GameManager;
    public GameObject ActionButtonPrefab;
    // Start is called before the first frame update
    void Start()
    {
        ActionsManager = GameObject.Find("_ActionsManager").GetComponent<ActionsManager>();
        GameManager = GameObject.Find("_GameManager").GetComponent<GameManager>();
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void CreatePuck(string action_id, InteractiveObject originalIO)
    {
        GameManager.SpawnPuck(action_id, CurrentActionPoint, originalIO);
    }

    public void SaveID(string new_id)
    {
        CurrentActionPoint.GetComponent<ActionPoint>().id = new_id;
    }

    public void UpdateMenu()
    {

        ActionPoint actionPoint;
        if (CurrentActionPoint == null)
        {
            return;
        } 
        else
        {
            actionPoint = CurrentActionPoint.GetComponent<ActionPoint>();
        }

        foreach (RectTransform o in transform.Find("Layout").Find("DynamicContent").GetComponentsInChildren<RectTransform>())
        {
            if (o.gameObject.tag != "Persistent")
            {
                Destroy(o.gameObject);
            }
        }
        transform.Find("Layout").Find("TopText").GetComponentInChildren<Text>().text = actionPoint.id;
        transform.Find("Layout").Find("InteractiveObjectType").GetComponent<Text>().text = actionPoint.IntObj.GetComponent<InteractiveObject>().type;
        
        foreach (KeyValuePair<InteractiveObject, List<ActionMetadata>> keyval in ActionsManager.GetAllActionsOfObject(actionPoint.IntObj.GetComponent<InteractiveObject>()))
        {
            foreach (ActionMetadata am in keyval.Value)
            {
                GameObject btnGO = Instantiate(GameManager.ButtonPrefab);
                btnGO.transform.SetParent(transform.Find("Layout").Find("DynamicContent"));
                btnGO.transform.localScale = new Vector3(1, 1, 1);
                Button btn = btnGO.GetComponent<Button>();
                btn.GetComponentInChildren<Text>().text = keyval.Key.Id + "/" + am.Name;
                btn.onClick.AddListener(() => CreatePuck(am.Name, keyval.Key));
            }

        }
        Dropdown dropdown = transform.Find("Layout").Find("RobotsList").GetComponent<Dropdown>();
        dropdown.options.Clear();
        dropdown.captionText.text = "";
        foreach (InteractiveObject interactiveObject in GameManager.InteractiveObjects.GetComponentsInChildren<InteractiveObject>())
        {            
            if (interactiveObject.ActionObjectMetadata.Robot)
            {
                Dropdown.OptionData option = new Dropdown.OptionData();
                option.text = interactiveObject.Id;
                dropdown.options.Add(option);                
            }
        }
        dropdown.value = 0;
        if (dropdown.options.Count > 0)
        {
            dropdown.interactable = true;
            transform.Find("Layout").Find("UpdatePositionButton").GetComponent<Button>().interactable = true;
            dropdown.captionText.text = dropdown.options[dropdown.value].text;
        } else
        {
            dropdown.interactable = false;
            transform.Find("Layout").Find("UpdatePositionButton").GetComponent<Button>().interactable = false;
        }
    }

    public void DeleteAP()
    {
        if (CurrentActionPoint == null)
            return;
        CurrentActionPoint.GetComponent<ActionPoint>().DeleteAP();
    }

    public void UpdateActionPointPosition()
    {
        Dropdown dropdown = transform.Find("Layout").Find("RobotsList").GetComponent<Dropdown>();
        GameManager.UpdateActionPointPosition(CurrentActionPoint.GetComponent<ActionPoint>(), dropdown.options[dropdown.value].text);
    }
}
