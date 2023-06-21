using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

[RequireComponent(typeof(Button))]
public class DialogButton : MonoBehaviour {
    [SerializeField]
    private TMPro.TMP_Text normal, highlighted;
    public Button btn;
    private ButtonWithTooltip buttonWithTooltip;
    private List<InteractiveObject> objectsToBeUnlocked;

    private void Awake() {
        btn = GetComponent<Button>();
        buttonWithTooltip = GetComponent<ButtonWithTooltip>();
    }

    public void SetLabel(string text) {
        normal.text = text;
        highlighted.text = text;
    }

    public void AddListener(UnityAction callback) {
        btn.onClick.AddListener(callback);
    }

    public void Init(string label, UnityAction callback) {
        SetLabel(label);
        AddListener(callback);
    }

    /// <summary>
    /// Button is interactive only if all of the objectsToBeUnlocked are not locked by other user
    /// </summary>
    /// <param name="label"></param>
    /// <param name="callback">Callback on click</param>
    /// <param name="objectsToBeUnlocked"></param>
    public void Init(string label, UnityAction callback, List<InteractiveObject> objectsToBeUnlocked) {
        SetLabel(label);
        AddListener(callback);
        this.objectsToBeUnlocked = objectsToBeUnlocked;
        Base.LockingEventsCache.Instance.OnObjectLockingEvent += OnObjectLockingEvent;
        OnObjectLockingEvent(null, null);
    }

    private void OnObjectLockingEvent(object sender, Base.ObjectLockingEventArgs args) {
        foreach (var obj in objectsToBeUnlocked) {
            if (obj.IsLockedByOtherUser) {
                SetInteractivity(false, "Start or end is locked");
                return;
            }
        }
        SetInteractivity(true);
    }

    public void SetInteractivity(bool interactable) {
        buttonWithTooltip.SetInteractivity(interactable);
    }

    public void SetInteractivity(bool interactable, string alternativeDescription) {
        buttonWithTooltip.SetInteractivity(interactable, alternativeDescription);
    }

    private void OnDestroy() {
        Base.LockingEventsCache.Instance.OnObjectLockingEvent -= OnObjectLockingEvent;
    }
}
