using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

public class InputDialogWithToggle : InputDialog {
    public Toggle GenerateLogicToggle;

    public bool GetToggleValue() {
        return GenerateLogicToggle.isOn;
    }
}
