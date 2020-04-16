using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Michsky.UI.ModernUIPack;

[RequireComponent(typeof(TooltipContent))]
public class ManualTooltip : MonoBehaviour
{
    [SerializeField]
    private TooltipContent tooltipContent;
    [SerializeField]
    private string description;

    private void Start() {
        Debug.Assert(tooltipContent != null);
        if (string.IsNullOrEmpty(description)) {
            Destroy(this);
            Destroy(tooltipContent);
            return;
        }
        tooltipContent.tooltipRect = TooltipRef.Instance.Tooltip;
        tooltipContent.descriptionText = TooltipRef.Instance.Text;
        tooltipContent.description = description;
    }
}
