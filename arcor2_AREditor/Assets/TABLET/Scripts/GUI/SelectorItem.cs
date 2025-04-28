using System.Collections.Generic;
using Base;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class SelectorItem : MonoBehaviour
{
    public TMP_Text Label;
    public Image Icon;
    public InteractiveObject InteractiveObject;
    public Button Button;
    public Image SelectionBorder, ManualSelector;
    public float Score;
    private long lastUpdate;
    private bool selected;

    public Button CollapsableButton;
    public GameObject CollapsableButtonIcon;
    public bool Collapsable, Collapsed;
    public GameObject SublistContent;
    public ManualTooltip Tooltip;




    public bool Removed; // TODO: set to true when removed from selectoraimmenu, 

    private List<SelectorItem> childs = new();

    public SelectorItem ParentItem;


    private void Start() {
        Removed = false;
        UpdateTooltip();
    }
    public void SetText(string text) {
        Label.text = text;
        name = text;
        UpdateTooltip();
    }

    public void UpdateTooltip() {
        Label.ForceMeshUpdate(true);
        if (Label.isTextTruncated) {
            Tooltip.Description = name;
            Tooltip.EnableTooltip();
            Tooltip.ShowDefaultDescription();
        } else {
            Tooltip.DisableTooltip();
        }
    }

    public void AddChild(SelectorItem selectorItem) {
        childs.Add(selectorItem);
        selectorItem.ParentItem = this;
        CollapsableButton.interactable = true;
    }

    public void RemoveChild(SelectorItem selectorItem) {
        childs.Remove(selectorItem);
        selectorItem.ParentItem = null;
        CollapsableButton.interactable = HasChilds();
    }

    public bool HasChilds() {
        return childs.Count > 0;
    }

    public void SetObject(InteractiveObject interactiveObject, float score, long currentIteration) {
        InteractiveObject = interactiveObject;
        Collapsable = false;
        Score = score;
        Button.onClick.AddListener(() => SelectorMenu.Instance.SetSelectedObject(this, true));
        lastUpdate = currentIteration;
        if (interactiveObject.GetType() == typeof(RobotActionObject)) {
            Collapsable = true;
            Icon.sprite = AREditorResources.Instance.Robot;
        } else if (interactiveObject.GetType().IsSubclassOf(typeof(ActionObject))) {
            Icon.sprite = AREditorResources.Instance.ActionObject;
        } else if (interactiveObject.GetType() == typeof(PuckInput)) {
            Icon.sprite = AREditorResources.Instance.ActionInput;
        } else if (interactiveObject.GetType() == typeof(PuckOutput)) {
            Icon.sprite = AREditorResources.Instance.ActionOutput;
        } else if (interactiveObject.GetType().IsSubclassOf(typeof(Action))) {
            Icon.sprite = AREditorResources.Instance.Action;
        } else if (interactiveObject.GetType().IsSubclassOf(typeof(ActionPoint))) {
            Collapsable = true;
            Icon.sprite = AREditorResources.Instance.ActionPoint;
        } else if (interactiveObject.GetType() == typeof(RobotEE)) {
            Icon.sprite = AREditorResources.Instance.RobotEE;
        } else if (interactiveObject.GetType() == typeof(APOrientation)) {
            Icon.sprite = AREditorResources.Instance.Orientation;
        } else {
            Icon.sprite = AREditorResources.Instance.Others;
        }
        if (!Collapsable)
            CollapsableButton.gameObject.SetActive(false);
    }

    public void UpdateScore(float score, long currentIteration) {
        lastUpdate = currentIteration;
        Score = score;
        //Label.text = $"{name}: {lastUpdate}, {VisibleChilds.Count}, {score}";
    }

    public long GetLastUpdate() {
        return lastUpdate;
    }

    public void SetSelected(bool selected, bool manually) {
        if (InteractiveObject != null) {
            if ((!this.selected && selected) || (selected && manually)) {
                InteractiveObject.SendMessage("OnHoverStart", SendMessageOptions.DontRequireReceiver);
            } else if (this.selected && !selected) {
                InteractiveObject.SendMessage("OnHoverEnd", SendMessageOptions.DontRequireReceiver);
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
        SetCollapsedState(!Collapsed);
    }

    public void SetCollapsedState(bool collapsed) {
        if (!Collapsable)
            return;
        Collapsed = collapsed;
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
