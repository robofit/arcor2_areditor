using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class CollapsableMenu : MonoBehaviour
{
    public string Name;
    [SerializeField]
    private bool collapsed;
    public GameObject CollapseButton, Content;
    [SerializeField]
    private TMPro.TMP_Text text;

    private string downArrow = " - ", rightArrow = " + ";

    public bool Collapsed {
        get => collapsed;
        set => SetCollapsedState(value);
    }

    void Start()
    {
        SetCollapsedState(collapsed);        
    }

    void Update()
    {

    }

    public void SetCollapsedState(bool state) {
        collapsed = state;
        Content.SetActive(!state);
        if (Collapsed) {
            text.text = rightArrow + Name;
        } else {
            text.text = downArrow + Name;
        }
    }

    public void ToggleCollapsedState()
    {
        SetCollapsedState(Content.activeSelf);
    }

}
