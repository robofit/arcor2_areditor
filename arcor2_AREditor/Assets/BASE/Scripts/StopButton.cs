using System;
using Arcor2.ClientSdk.Communication;
using Base;
using UnityEngine;
using Action = Base.Action;

public class StopButton : MonoBehaviour {
    private void Start() {
        GameManager.Instance.OnActionExecution += OnActionExecution;
        GameManager.Instance.OnActionExecutionFinished += OnActionExecutionFinishedOrCancelled;
        GameManager.Instance.OnActionExecutionCanceled += OnActionExecutionFinishedOrCancelled;
        gameObject.SetActive(false);
    }

    private void OnActionExecutionFinishedOrCancelled(object sender, EventArgs e) {
        gameObject.SetActive(false);
    }

    private void OnActionExecution(object sender, ActionExecutionEventArgs args) {
        try {
            Action action = ProjectManager.Instance.GetAction(args.Data.ActionId);
            if (action.ActionProvider.IsRobot() && action.Metadata.Meta.Cancellable)
                gameObject.SetActive(true);
        } catch (ItemNotFoundException ex) {

        }

    }

    public async void CancelExecution() {
        await GameManager.Instance.CancelExecution();
    }
}
