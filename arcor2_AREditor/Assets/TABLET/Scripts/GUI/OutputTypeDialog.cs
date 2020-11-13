using System.Collections;
using System.Collections.Generic;
using Base;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

public class OutputTypeDialog : Dialog
{
    public InputOutput InputOutput;
    private UnityAction confirmDialog;

    public void Open(InputOutput puckOutput, UnityAction confirmDialog) {
        InputOutput = puckOutput;
        this.confirmDialog = confirmDialog;
        Open();
    }

    public Toggle Any, True, False;
    public override void Confirm() {
        Close();
        if (Any.isOn)
            InputOutput.ifValue = null;
        else if (True.isOn)
            InputOutput.ifValue = true;
        else if (False.isOn)
            InputOutput.ifValue = false;
        confirmDialog.Invoke();
    }

   
}
