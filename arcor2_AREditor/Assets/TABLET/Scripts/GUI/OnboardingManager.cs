using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class OnboardingManager : MonoBehaviour
{
    public static OnboardingManager Isntance;

    private void Awake() {
        Isntance = this;
    }
    /*
    public List<WalktroughStep> WalktroughSteps = new List<WalktroughStep>() {
        new WalktroughStep() {
            Order = 0,
            PrimaryText = "Let's create our first scene tap on higlighted button.",
            SecondaryText = "",
            Tip = "",
            HighlitedButton = null,
        },

        new WalktroughStep() {
            Order = 1,
            PrimaryText = "Create name and then press done.",
            SecondaryText = "This is some se ",
            Tip = "",
            HighlitedButton = null,
        }
    };
    */
}
