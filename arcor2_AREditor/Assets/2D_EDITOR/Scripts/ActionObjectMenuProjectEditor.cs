using UnityEngine;
using UnityEngine.UI;
using Base;
using UnityEngine.EventSystems;

public class ActionObjectMenuProjectEditor : ActionObjectMenu {
    public GameObject DynamicContent;

    [SerializeField]
    private InputDialog inputDialog;

    [SerializeField]
    private ButtonWithTooltip createAPBtn;
    
    public async void CreateNewAP(string name) {
        Debug.Assert(CurrentObject != null);
        
        Vector3 abovePoint = SceneManager.Instance.GetCollisionFreePointAbove(CurrentObject.transform, Vector3.one * 0.025f, Quaternion.identity);        
        IO.Swagger.Model.Position offset = DataHelper.Vector3ToPosition(TransformConvertor.UnityToROS(abovePoint));
        bool result = await GameManager.Instance.AddActionPoint(name, CurrentObject.Data.Id, offset);
        if (result)
            inputDialog.Close();
        UpdateMenu();
    }

    public void ShowAddActionPointDialog() {
        inputDialog.Open("Create action point",
                         "Type action point name",
                         "Name",
                         ProjectManager.Instance.GetFreeAPName(CurrentObject.Data.Name),
                         () => CreateNewAP(inputDialog.GetValue()),
                         () => inputDialog.Close());
    }

    public override void UpdateMenu() {
        base.UpdateMenu();



        foreach (Transform t in DynamicContent.transform) {
            Destroy(t.gameObject);
        }
        createAPBtn.SetInteractivity(CurrentObject.ActionObjectMetadata.HasPose);

        foreach (ActionPoint actionPoint in CurrentObject.GetActionPoints()) {
            Button button = GameManager.Instance.CreateButton(DynamicContent.transform, actionPoint.Data.Name);
            button.onClick.AddListener(() => ShowActionPoint(actionPoint));

            // Add EventTrigger OnPointerEnter and OnPointerExit - to be able to highlight corresponding AP when hovering over button
            OutlineOnClick APoutline = actionPoint.GetComponent<OutlineOnClick>();
            EventTrigger eventTrigger = button.gameObject.AddComponent<EventTrigger>();
            // Create OnPointerEnter entry
            EventTrigger.Entry OnPointerEnter = new EventTrigger.Entry {
                eventID = EventTriggerType.PointerEnter
            };
            OnPointerEnter.callback.AddListener((eventData) => APoutline.Highlight());
            eventTrigger.triggers.Add(OnPointerEnter);

            // Create OnPointerExit entry
            EventTrigger.Entry OnPointerExit = new EventTrigger.Entry {
                eventID = EventTriggerType.PointerExit
            };
            OnPointerExit.callback.AddListener((eventData) => APoutline.UnHighlight());
            eventTrigger.triggers.Add(OnPointerExit);
        }
    }

    private static void ShowActionPoint(ActionPoint actionPoint) {
        MenuManager.Instance.ActionObjectMenuProjectEditor.Close();
        actionPoint.ShowMenu(true);
        
        SceneManager.Instance.SetSelectedObject(actionPoint.gameObject);
        // Select(force = true) to force selection and not losing AP highlight upon ActionObjectMenuProjectEditor menu closing 
        actionPoint.SendMessage("Select", true);
    }

    public void OverrideParameters() {

    }

    protected override void UpdateSaveBtn() {
        if (SceneManager.Instance.SceneStarted) {
            SaveParametersBtn.SetInteractivity(false, "Parameters could be overrided only when scene is stopped.");
            return;
        }
        if (!parametersChanged) {
            SaveParametersBtn.SetInteractivity(false, "No parameter changed");
            return;
        }
        // TODO: add dry run save
        SaveParametersBtn.SetInteractivity(true);

    }
}
