using Base;
using Michsky.UI.ModernUIPack;
using TMPro;
using UnityEngine;
using UnityEngine.Events;
using Pose = Arcor2.ClientSdk.Communication.OpenApi.Models.Pose;

public class PoseEvent : UnityEvent<Pose> {

}

public class RelPoseParam : MonoBehaviour, IParameter {

    [SerializeField]
    private TMP_Text label;
    private TooltipContent tooltipContent;

    [SerializeField]
    private LabeledInput posX, posY, posZ, orX, orY, orZ, orW;

    public PoseEvent OnValueChangedEvent = new();

    private Pose pose = null;

    private void Awake() {
        Debug.Assert(label != null);
        Debug.Assert(posX != null);
        Debug.Assert(posY != null);
        Debug.Assert(posZ != null);
        Debug.Assert(orX != null);
        Debug.Assert(orY != null);
        Debug.Assert(orZ != null);
        Debug.Assert(orW != null);
        tooltipContent = label.GetComponent<TooltipContent>();
        tooltipContent.tooltipRect = GameManager.Instance.Tooltip;
        tooltipContent.descriptionText = GameManager.Instance.Text;
    }


    public string GetName() {
        return label.text;
    }

    public object GetValue() {
        return GetPose();
    }

    public Pose GetPose() {
        return pose;
        /*double posXValue = (double) posX.GetValue(),
                    posYValue = (double) posY.GetValue(),
                    posZValue = (double) posZ.GetValue(),
                    orXValue = (double) orX.GetValue(),
                    orYValue = (double) orY.GetValue(),
                    orZValue = (double) orZ.GetValue(),
                    orWValue = (double) orW.GetValue();
        return new IO.Swagger.Model.Pose(new IO.Swagger.Model.Orientation(x: (decimal) orXValue, y: (decimal) orYValue, z: (decimal) orZValue, w: (decimal) orWValue),
            new IO.Swagger.Model.Position(x: (decimal) posXValue, y: (decimal) posYValue, z: (decimal) posZValue));*/
    }

    public void SetLabel(string label, string description) {
        this.label.text = label;
        tooltipContent.description = description;
    }

    public void SetValue(object value) {
        var pose = (Pose) value;
        this.pose = pose;
        posX.SetValue(pose.Position.X);
        posY.SetValue(pose.Position.Y);
        posZ.SetValue(pose.Position.Z);
        orX.SetValue(pose.Orientation.X);
        orY.SetValue(pose.Orientation.Y);
        orZ.SetValue(pose.Orientation.Z);
        orW.SetValue(pose.Orientation.W);
    }



    public void OnValueChagned(string paramId) {
        if (pose == null)
            return;
        switch (paramId) {
            case "posX":
                pose.Position.X = (decimal) (double) posX.GetValue();
                break;
            case "posY":
                pose.Position.Y = (decimal) (double) posY.GetValue();
                break;
            case "posZ":
                pose.Position.Z = (decimal) (double) posZ.GetValue();
                break;
            case "orX":
                pose.Orientation.X = (decimal) (double) orX.GetValue();
                break;
            case "orY":
                pose.Orientation.Y = (decimal) (double) orY.GetValue();
                break;
            case "orZ":
                pose.Orientation.Z = (decimal) (double) orZ.GetValue();
                break;
            case "orW":
                pose.Orientation.W = (decimal) (double) orW.GetValue();
                break;
        }
        OnValueChangedEvent.Invoke(pose);
    }

    public void SetDarkMode(bool dark) {
        posX.SetDarkMode(dark);
        posY.SetDarkMode(dark);
        posZ.SetDarkMode(dark);
        orX.SetDarkMode(dark);
        orY.SetDarkMode(dark);
        orZ.SetDarkMode(dark);
        orW.SetDarkMode(dark);
    }

    public string GetCurrentType() {
        return "relative_pose";
    }

    public Transform GetTransform() {
        return transform;
    }

    public void SetInteractable(bool interactable) {
        posX.SetInteractable(interactable);
        posY.SetInteractable(interactable);
        posZ.SetInteractable(interactable);
        orX.SetInteractable(interactable);
        orY.SetInteractable(interactable);
        orZ.SetInteractable(interactable);
        orW.SetInteractable(interactable);
    }
}
