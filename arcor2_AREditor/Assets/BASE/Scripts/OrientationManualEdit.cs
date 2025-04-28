using System;
using System.Globalization;
using Arcor2.ClientSdk.Communication.OpenApi.Models;
using TMPro;
using UnityEngine;

public class OrientationManualEdit : MonoBehaviour {
    public TMP_InputField InputX, InputY, InputZ, InputW; //for quaternion/euler input
    public GameObject InputFieldW; //for setting (in)active

    private Orientation orientation = null;
    private bool eulerMode = false; //true for orientation edited in euler angles, false for quaternion


    public void SetOrientation(Orientation orientation) {
        this.orientation = orientation;
        NumberFormatInfo numberFormatInfo = new();
        numberFormatInfo.NumberDecimalSeparator = ".";
        InputFieldW.SetActive(!eulerMode);

        if (eulerMode) {
            Vector3 euler = new Quaternion((float) orientation.X, (float) orientation.Y, (float) orientation.Z, (float) orientation.W).eulerAngles;
            InputX.text = euler.x.ToString("F10", numberFormatInfo);
            InputY.text = euler.y.ToString("F10", numberFormatInfo);
            InputZ.text = euler.z.ToString("F10", numberFormatInfo);

        } else {
            InputX.text = orientation.X.ToString(numberFormatInfo);
            InputY.text = orientation.Y.ToString(numberFormatInfo);
            InputZ.text = orientation.Z.ToString(numberFormatInfo);
            InputW.text = orientation.W.ToString(numberFormatInfo);
        }
    }

    public Orientation GetOrientation() {
        Orientation orientation;
        if (eulerMode) {
            float x = float.Parse(InputX.text, NumberStyles.AllowDecimalPoint | NumberStyles.AllowLeadingSign, CultureInfo.InvariantCulture);
            float y = float.Parse(InputY.text, NumberStyles.AllowDecimalPoint | NumberStyles.AllowLeadingSign, CultureInfo.InvariantCulture);
            float z = float.Parse(InputZ.text, NumberStyles.AllowDecimalPoint | NumberStyles.AllowLeadingSign, CultureInfo.InvariantCulture);
            Quaternion q = Quaternion.Euler(x, y, z);
            orientation = new Orientation(Convert.ToDecimal(q.w), Convert.ToDecimal(q.x), Convert.ToDecimal(q.y), Convert.ToDecimal(q.z));
        } else {
            decimal x = decimal.Parse(InputX.text, NumberStyles.AllowDecimalPoint | NumberStyles.AllowLeadingSign, CultureInfo.InvariantCulture);
            decimal y = decimal.Parse(InputY.text, NumberStyles.AllowDecimalPoint | NumberStyles.AllowLeadingSign, CultureInfo.InvariantCulture);
            decimal z = decimal.Parse(InputZ.text, NumberStyles.AllowDecimalPoint | NumberStyles.AllowLeadingSign, CultureInfo.InvariantCulture);
            decimal w = decimal.Parse(InputW.text, NumberStyles.AllowDecimalPoint | NumberStyles.AllowLeadingSign, CultureInfo.InvariantCulture);
            orientation = new Orientation(w, x, y, z);
        }
        this.orientation = orientation;
        return orientation;
    }

    public void SetEulerMode() {
        eulerMode = true;
        SetOrientation(orientation);
    }

    public void SetQuaternionMode() {
        eulerMode = false;
        SetOrientation(orientation);
    }

    public string ValidateFields() {
        string tooltipDescription = "";

        if (string.IsNullOrEmpty(InputX.text) || string.IsNullOrEmpty(InputY.text) || string.IsNullOrEmpty(InputZ.text)) {
            tooltipDescription = "All values are required";
        }

        if (!eulerMode) {
            if (string.IsNullOrEmpty(InputW.text)) {
                tooltipDescription = "All values are required";
            }
        }

        if (string.IsNullOrEmpty(tooltipDescription)) {
            try {
                decimal.Parse(InputX.text, NumberStyles.AllowDecimalPoint | NumberStyles.AllowLeadingSign, CultureInfo.InvariantCulture);
                decimal.Parse(InputY.text, NumberStyles.AllowDecimalPoint | NumberStyles.AllowLeadingSign, CultureInfo.InvariantCulture);
                decimal.Parse(InputZ.text, NumberStyles.AllowDecimalPoint | NumberStyles.AllowLeadingSign, CultureInfo.InvariantCulture);
                if (!eulerMode) {
                    decimal.Parse(InputW.text, NumberStyles.AllowDecimalPoint | NumberStyles.AllowLeadingSign, CultureInfo.InvariantCulture);
                }
            } catch (Exception ex) {
                tooltipDescription = ex.Message;
            }
        }
        return tooltipDescription;
    }
}
