using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using Base;
using IO.Swagger.Model;
using Michsky.UI.ModernUIPack;
using UnityEngine;
using UnityEngine.UI;

public class PositionManualEdit : MonoBehaviour
{
    public TMPro.TMP_InputField InputX, InputY, InputZ;

    [SerializeField]
    private Button ConfirmButton;

    [SerializeField]
    private TooltipContent buttonTooltip;

    public void SetPosition(Position position) {
        NumberFormatInfo numberFormatInfo = new NumberFormatInfo();
        numberFormatInfo.NumberDecimalSeparator = ".";

        InputX.text = position.X.ToString(numberFormatInfo);
        InputY.text = position.Y.ToString(numberFormatInfo);
        InputZ.text = position.Z.ToString(numberFormatInfo);

        buttonTooltip.description = "First, edit the position";
        buttonTooltip.enabled = true;
        ConfirmButton.interactable = false;
    }

    public Position GetPosition() {
        decimal x = decimal.Parse(InputX.text, NumberStyles.AllowDecimalPoint | NumberStyles.AllowLeadingSign, CultureInfo.InvariantCulture);
        decimal y = decimal.Parse(InputY.text, NumberStyles.AllowDecimalPoint | NumberStyles.AllowLeadingSign, CultureInfo.InvariantCulture);
        decimal z = decimal.Parse(InputZ.text, NumberStyles.AllowDecimalPoint | NumberStyles.AllowLeadingSign, CultureInfo.InvariantCulture);
        return new Position(x, y, z);
    }

    public async void ValidateFields() {
        bool interactable = true;

        if (string.IsNullOrEmpty(InputX.text) || string.IsNullOrEmpty(InputY.text) || string.IsNullOrEmpty(InputZ.text)) {
            buttonTooltip.description = "All values are required";
            interactable = false;
        } else {
            try {
                decimal.Parse(InputX.text, NumberStyles.AllowDecimalPoint | NumberStyles.AllowLeadingSign, CultureInfo.InvariantCulture);
                decimal.Parse(InputY.text, NumberStyles.AllowDecimalPoint | NumberStyles.AllowLeadingSign, CultureInfo.InvariantCulture);
                decimal.Parse(InputZ.text, NumberStyles.AllowDecimalPoint | NumberStyles.AllowLeadingSign, CultureInfo.InvariantCulture);
            } catch (Exception ex) {
                interactable = false;
                buttonTooltip.description = ex.Message;
            }
        }

        buttonTooltip.enabled = !interactable;
        ConfirmButton.interactable = interactable;
    }
}
