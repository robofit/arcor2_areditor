using System.Collections;
using System.Collections.Generic;
using Base;
using IO.Swagger.Model;
using UnityEngine;
using UnityEngine.UI;

public class SelectorItem : MonoBehaviour
{
    public TMPro.TMP_Text Label;
    public Image Icon;
    public InteractiveObject InteractiveObject;
    public Button Button;
    public Image SelectionBorder, ManualSelector;
    public float Score;
    private long lastUpdate;
    private bool selected;
    public Sprite ActionPoint, ActionObject, Robot, RobotEE, Orientation, ActionInput, ActionOutput, Action, Others;

    public Button CollapsableButton;
    public GameObject CollapsableButtonIcon;
    public bool Collapsable, Collapsed;
    public GameObject SublistContent;

    private int deep;


    public int Deep {
        get => deep;
        set {
            deep = value;
            RectTransform r = (RectTransform) transform;
            r.rect.Set(r.rect.x, r.rect.y, 550 - deep * 25, r.rect.height);
        }
    }

    private void Awake() {
    }


    public void SetText(string text) {
        Label.text = text;
    }
    public void SetObject(InteractiveObject interactiveObject, float score, long currentIteration) {
        InteractiveObject = interactiveObject;
        Collapsable = false;
        Score = score;
        Button.onClick.AddListener(() => SelectorMenu.Instance.SetSelectedObject(this, true));
        lastUpdate = currentIteration;
        if (interactiveObject.GetType() == typeof(RobotActionObject)) {
            Icon.sprite = Robot;
        } else if (interactiveObject.GetType().IsSubclassOf(typeof(ActionObject))) {
            Icon.sprite = ActionObject;
        } else if (interactiveObject.GetType() == typeof(PuckInput)) {
            Icon.sprite = ActionInput;
        } else if (interactiveObject.GetType() == typeof(PuckOutput)) {
            Icon.sprite = ActionOutput;
        } else if (interactiveObject.GetType().IsSubclassOf(typeof(Base.Action))) {
            Collapsable = true;
            Icon.sprite = Action;
        } else if (interactiveObject.GetType().IsSubclassOf(typeof(Base.ActionPoint))) {
            Collapsable = true;
            Icon.sprite = ActionPoint;
        } else if (interactiveObject.GetType() == typeof(RobotEE)) {
            Icon.sprite = RobotEE;
        } else if (interactiveObject.GetType() == typeof(APOrientation)) {
            Icon.sprite = Orientation;
        } else {
            Icon.sprite = Others;
        }
        if (!Collapsable)
            CollapsableButton.gameObject.SetActive(false);
    }

    public void UpdateScore(float score, long currentIteration) {
        lastUpdate = currentIteration;
        Score = score;
    }

    public long GetLastUpdate() {
        return lastUpdate;
    }

    public void SetSelected(bool selected, bool manually) {
        if (InteractiveObject != null) {

            if (!this.selected && selected) {
                InteractiveObject.SendMessage("OnHoverStart");
            } else if (this.selected && !selected) {
                InteractiveObject.SendMessage("OnHoverEnd");
            }
        }   
        this.selected = selected;
        if (manually) {
            SelectionBorder.gameObject.SetActive(false);
            ManualSelector.gameObject.SetActive(selected);
        } else {
            ManualSelector.gameObject.SetActive(false);
            SelectionBorder.gameObject.SetActive(selected);
        }
    }

    public bool IsSelected() {
        return selected;
    }

    public void CollapseBtnCb() {
        Collapsed = !Collapsed;
        //ActionPoint3D actionPoint = (ActionPoint3D) InteractiveObject;
        if (Collapsed) {
            CollapsableButtonIcon.transform.rotation = Quaternion.Euler(new Vector3(0, 0, 180));
            //actionPoint.ActionsCollapsed = true;
            //actionPoint.UpdatePositionsOfPucks();
            SublistContent.SetActive(false);
        } else {
            CollapsableButtonIcon.transform.rotation = Quaternion.Euler(new Vector3(0, 0, 90));
            //actionPoint.ActionsCollapsed = false;
            //actionPoint.UpdatePositionsOfPucks();

            SublistContent.SetActive(true);
            ///StartCoroutine(UpdateSubitem()); // HACK
        }
    }

   


}
