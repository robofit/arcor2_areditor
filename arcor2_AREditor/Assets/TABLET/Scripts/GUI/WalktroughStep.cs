using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WalktroughStep
{
    public int Order {
        get; set;
    }
    public int Progress {
        get; set;
    }
    public bool Skippable {
        get; set;
    }
    public string Tip {
        get; set;
    }
    public string PrimaryText {
        get; set;
    }
    public string SecondaryText {
        get; set;
    }
    public GameObject HighlitedButton {
        get; set;
    }

}
