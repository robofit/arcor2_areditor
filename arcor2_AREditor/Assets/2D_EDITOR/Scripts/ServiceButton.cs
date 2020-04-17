using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Michsky.UI.ModernUIPack;

public class ServiceButton : MonoBehaviour
{
    //public string Type;
    public IO.Swagger.Model.ServiceTypeMeta ServiceMetadata;
    public GameObject Yes, No;
    private bool state;
    [SerializeField]
    private Button button;
    [SerializeField]
    private TooltipContent tooltipContent;

    private void Start() {
        tooltipContent.tooltipRect = TooltipRef.Instance.Tooltip;
        tooltipContent.descriptionText = TooltipRef.Instance.Text;
    }

    public bool State {
        get => state;
        set {
            state = value;
            if (state) {
                No.SetActive(false);
                Yes.SetActive(true);
            } else {
                Yes.SetActive(false);
                No.SetActive(true);
            }
        }
    }

    public void SetInteractable(bool interactable) {
        button.interactable = interactable;
        if (!interactable) {
            tooltipContent.enabled = true;
            tooltipContent.description = ServiceMetadata.Description + " " + ServiceMetadata.Problem;
        } else {
            tooltipContent.enabled = false;
        }
    }

}
