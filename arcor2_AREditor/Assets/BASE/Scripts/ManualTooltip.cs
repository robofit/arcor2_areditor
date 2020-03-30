using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Michsky.UI.ModernUIPack;

public class ManualTooltip : MonoBehaviour
{
    [SerializeField]
    private TooltipContent tooltipContent;
    [SerializeField]
    private string description;

    private void Start() {
        tooltipContent.tooltipRect = TooltipRef.Instance.Tooltip;
        tooltipContent.descriptionText = TooltipRef.Instance.Text;
        tooltipContent.description = description;
    }
}
