using Base;
using UnityEngine.Events;
using UnityEngine.UI;

public class OutputTypeDialog : Dialog {
    public InputOutput InputOutput;
    private UnityAction confirmDialog;

    public void Open(InputOutput puckOutput, UnityAction confirmDialog, bool any, bool @true, bool @false) {
        Any.gameObject.SetActive(any);
        True.gameObject.SetActive(@true);
        False.gameObject.SetActive(@false);
        if (@true && !@false) {
            False.isOn = false;
            Any.isOn = false;
            True.isOn = true;
        } else if (@false && !@true) {
            True.isOn = false;
            Any.isOn = false;
            False.isOn = true;
        }
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
