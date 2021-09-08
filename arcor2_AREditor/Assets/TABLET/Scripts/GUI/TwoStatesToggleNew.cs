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
        /*icon1 = BigImage.sprite;
        icon2 = SmallImage.sprite;*/
        CurrentState = States.Left;
    }
    public Image LeftImage, RightImage;
    private Sprite icon1, icon2;
    public string State1, State2;
    [HideInInspector]
    public States CurrentState;
    public Animator Animator;

    public UnityEvent OnLeft, OnRight;

    public void SwitchToLeft() {
        if (CurrentState == States.Left)
            return;
        CurrentState = States.Left;
        OnLeft?.Invoke();
        //Animator.SetTrigger("SwitchToLeft");
        Animator.Play("SwitchToLeft");
    }

    public void SwitchToRight() {
        if (CurrentState == States.Right)
            return;
        CurrentState = States.Right;
        OnRight?.Invoke();
        //Animator.SetTrigger("SwitchToRight");
        Animator.Play("SwitchToRight");
    }

    public void SetInteractivity(bool interactable) {
        //base.SetInteractivity(interactable);
        //SetImagesColors(interactable);
    }

    public void SetInteractivity(bool interactive, string alterateDescription = null) {
        //base.SetInteractivity(interactive, alterateDescription);
        //SetImagesColors(interactive);
    }

    /*private void SetImagesColors(bool interactable) {
        if (!interactable) {
            BigImage.color = Color.gray;
            SmallImage.color = Color.gray;
        } else {
            BigImage.color = Color.white;
            SmallImage.color = Color.white;
        }
    }*/

}
