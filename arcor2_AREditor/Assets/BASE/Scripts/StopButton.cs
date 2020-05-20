using System;
using System.Collections;
using System.Collections.Generic;
using Base;
using UnityEngine;

public class StopButton : MonoBehaviour
{
    private void Start() {
        Base.GameManager.Instance.OnActionExecution += OnActionExecution;
        Base.GameManager.Instance.OnActionExecutionFinished += OnActionExecutionFinishedOrCancelled;
        Base.GameManager.Instance.OnActionExecutionCanceled += OnActionExecutionFinishedOrCancelled;
        gameObject.SetActive(false);
    }

    private void OnActionExecutionFinishedOrCancelled(object sender, EventArgs e) {
        gameObject.SetActive(false);
    }

    private void OnActionExecution(object sender, StringEventArgs args) {
        try {
            Base.Action action = ProjectManager.Instance.GetAction(args.Data);
            if (action.ActionProvider.IsRobot() && action.Metadata.Meta.Cancellable)
                gameObject.SetActive(true);
        } catch (ItemNotFoundException ex) {

        } 
        
    }

    public async void CancelExecution() {
        await GameManager.Instance.CancelExecution();
    }
}
