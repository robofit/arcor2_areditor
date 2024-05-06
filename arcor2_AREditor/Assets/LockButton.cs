/*
 * LockButton
 * Author: Timotej Halenár
 * Login: xhalen00
 * Bachelor's Thesis 
 * VUT FIT 2024
 * 
 * */

using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class LockButton : MonoBehaviour
{
    public Sprite Locked;
    public Sprite Unlocked;
    private bool isLocked;

    public delegate void OnLocked();
    public delegate void OnUnlocked();

    public event OnLocked OnLockedEvent;
    public event OnUnlocked OnUnlockedEvent;

    public void ChangeToLocked() {
        GetComponent<Image>().sprite = Locked;
        isLocked = true;
        OnLockedEvent();
    }

    public void ChangeToUnlocked() {
        GetComponent<Image>().sprite = Unlocked;
        isLocked = false;
        OnUnlockedEvent();
    }

    public void OnButtonClick() {
        if (isLocked) {
            ChangeToUnlocked();
        } else {
            ChangeToLocked();
        }
    }

    
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
