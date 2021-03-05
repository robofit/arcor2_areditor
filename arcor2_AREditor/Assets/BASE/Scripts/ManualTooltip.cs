using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Michsky.UI.ModernUIPack;
using UnityEngine.EventSystems;

[RequireComponent(typeof(TooltipContent))]
public class ManualTooltip : MonoBehaviour {
    [SerializeField]
    private TooltipContent tooltipContent;
    [SerializeField]
    public string Description, DescriptionAlternative;
    [SerializeField]
    private bool displayAlternativeDescription = false;

    public bool DisplayAlternativeDescription {
        get => displayAlternativeDescription;
        set {
            displayAlternativeDescription = value;
            if (displayAlternativeDescription) {
                ShowAlternativeDescription();
            } else {
                ShowDefaultDescription();
            }
        }
    }

    private void Start() {
        Debug.Assert(tooltipContent != null);
        if (string.IsNullOrEmpty(Description)) {
            tooltipContent.enabled = false;
            return;
        }
        if (tooltipContent.tooltipRect == null || tooltipContent.descriptionText == null) {
            tooltipContent.tooltipRect = TooltipRef.Instance.Tooltip;
            tooltipContent.descriptionText = TooltipRef.Instance.Text;            
        }

        if (DisplayAlternativeDescription) {
            ShowAlternativeDescription();
        } else {
            ShowDefaultDescription();
        }
    }

    public void ShowDefaultDescription() {
        if (string.IsNullOrEmpty(Description)) {
            tooltipContent.enabled = false;
        } else {
            tooltipContent.description = Description;
            tooltipContent.enabled = true;
        }
    }

    public void ShowAlternativeDescription() {        
        if (string.IsNullOrEmpty(DescriptionAlternative)) {
            tooltipContent.enabled = false;
        } else {
            tooltipContent.description = DescriptionAlternative;
            tooltipContent.enabled = true;
        }
    }

    private void OnDisable() {
        tooltipContent.OnPointerExit(null);
    }

}
