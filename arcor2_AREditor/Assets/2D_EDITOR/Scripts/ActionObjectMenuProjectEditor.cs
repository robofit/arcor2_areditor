using UnityEngine;
using UnityEngine.UI;
using Base;

public class ActionObjectMenuProjectEditor : MonoBehaviour, IMenu {
    public Base.ActionObject CurrentObject;
    [SerializeField]
    private TMPro.TMP_Text objectName;
    public Slider VisibilitySlider;
    public GameObject DynamicContent;

    
    public void CreateNewAP() {
        if (CurrentObject == null) {
            return;
        }
        Base.Scene.Instance.SpawnActionPoint(CurrentObject.GetComponent<Base.ActionObject>(), null);
        UpdateMenu();
    }

    public void UpdateMenu() {
        objectName.text = CurrentObject.Data.Id;
        VisibilitySlider.value = CurrentObject.GetVisibility()*100;
        foreach (Transform t in DynamicContent.transform) {
            Destroy(t.gameObject);
        }
        foreach (ActionPoint actionPoint in CurrentObject.ActionPoints.Values) {
            Button button = GameManager.Instance.CreateButton(DynamicContent.transform, actionPoint.Data.Id);
            button.onClick.AddListener(() => ShowActionPoint(actionPoint));
        }
    }

    public void OnVisibilityChange(float value) {
        CurrentObject.SetVisibility(value/100f); 
    }

    public void ShowNextAO() {
        ActionObject nextAO = Scene.Instance.GetNextActionObject(CurrentObject.Data.Id);
        ShowActionObject(nextAO);
    }

    public void ShowPreviousAO() {
        ActionObject previousAO = Scene.Instance.GetNextActionObject(CurrentObject.Data.Id);
        ShowActionObject(previousAO);
    }

    private static void ShowActionObject(ActionObject actionObject) {
        actionObject.ShowMenu();
        Scene.Instance.SetSelectedObject(actionObject.gameObject);
        actionObject.SendMessage("Select");
    }

    private static void ShowActionPoint(ActionPoint actionPoint) {
        MenuManager.Instance.ActionObjectMenuProjectEditor.Close();
        actionPoint.ShowMenu();
        Scene.Instance.SetSelectedObject(actionPoint.gameObject);
        actionPoint.SendMessage("Select");
    }
}
