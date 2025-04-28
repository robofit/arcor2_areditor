using System;
using Arcor2.ClientSdk.Communication;
using Base;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class ProjectParameterButton : MonoBehaviour {
    public Button Button;
    [SerializeField]
    private ButtonWithTooltip ButtonWithTooltip;
    public TMP_Text Name, Value;
    public string Id;

    // Need to keep track, so we can properly unregister it
    private static EventHandler<ProjectParameterEventArgs> onProjectParameterAdded;

    // Start is called before the first frame update
    void Start() {
        onProjectParameterAdded = CommunicationManager.SafeEventHandler<ProjectParameterEventArgs>(OnProjectParameterUpdated);
        CommunicationManager.Instance.Client.ProjectParameterUpdated += onProjectParameterAdded;
        LockingEventsCache.Instance.OnObjectLockingEvent += OnLockingEvent;
    }

    private void OnLockingEvent(object sender, ObjectLockingEventArgs args) {
        if (!args.ObjectIds.Contains(Id))
            return;

        ButtonWithTooltip.SetInteractivity(!args.Locked && args.Owner != LandingScreen.Instance.GetUsername(), "Project parameter is being edited by " + args.Owner);
    }

    private void OnProjectParameterUpdated(object sender, ProjectParameterEventArgs args) {
        if (args.Data.Id != Id)
            return;

        SetName(args.Data.Name);
        SetValue(ProjectParametersHelper.GetValue(args.Data.Value, ProjectParametersHelper.ConvertStringParameterTypeToEnum(args.Data.Type)));
    }

    private void OnDestroy() {
        CommunicationManager.Instance.Client.ProjectParameterUpdated -= onProjectParameterAdded;
        LockingEventsCache.Instance.OnObjectLockingEvent -= OnLockingEvent;
    }

    internal void SetName(string name) {
        Name.SetText(name);
    }

    internal void SetValue(string value) {
        Value.SetText(value);
    }
    internal void SetValue(object value) {
        SetValue(value.ToString());
    }

}
