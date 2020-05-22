using System.Collections;
using System.Collections.Generic;
using Michsky.UI.ModernUIPack;
using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(Button))]
public class ButtonWithTooltip : MonoBehaviour {
    [SerializeField]
    private ManualTooltip tooltip;
    [SerializeField]
    private Button Button;
    [SerializeField]
    private TooltipContent TooltipContent;

    private void Start() {
        Button = gameObject.GetComponent<Button>();
        TooltipContent = gameObject.GetComponent<TooltipContent>();
    }

    public void SetInteractivity(bool interactable) {
        Button.interactable = interactable;
        if (interactable) {
            tooltip.ShowDefaultDescription();
        } else {
            tooltip.ShowAlternativeDescription();
        }
    }

    public void SetInteractivity(bool interactable, string alternativeDescription) {
        tooltip.DescriptionAlternative = alternativeDescription;
        SetInteractivity(interactable);
    }

    public void HideTooltip() {
        TooltipContent.tooltipAnimator.Play("Out");
    }
}
