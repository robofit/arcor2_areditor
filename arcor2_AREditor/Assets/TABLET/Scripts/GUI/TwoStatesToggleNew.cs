using System;
using System.Collections;
using System.Collections.Generic;
using UnityEditorInternal;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

public class TwoStatesToggleNew : MonoBehaviour
{
    public enum States {
        Left,
        Right
    }  
    
    private void Awake() {
        CurrentState = States.Left;
    }
    public Image LeftImage, RightImage;
    private Sprite icon1, icon2;
    public string State1, State2;
    [HideInInspector]
    public States CurrentState;
    public Animator Animator;

    public UnityEvent OnLeft, OnRight;

    public ManualTooltip DisabledTooltip;
    public GameObject DisabledImage;

    public void SwitchToLeft() {
        if (CurrentState == States.Left)
            return;
        CurrentState = States.Left;
        OnLeft?.Invoke();
        Animator.Play("SwitchToLeft");
    }

    public void SwitchToRight() {
        if (CurrentState == States.Right)
            return;
        CurrentState = States.Right;
        OnRight?.Invoke();
        Animator.Play("SwitchToRight");
    }

    public void SetInteractivity(bool interactable) {
        DisabledImage.SetActive(!interactable);
    }

    public void SetInteractivity(bool interactive, string alterateDescription = null) {
        SetInteractivity(interactive);
        DisabledTooltip.DescriptionAlternative = alterateDescription;
        DisabledTooltip.DisplayAlternativeDescription = !interactive;
    }


}
