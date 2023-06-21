using UnityEngine;
using UnityEngine.UI;

public class CollapsableMenu : MonoBehaviour {
    [SerializeField]
    private string Name;
    [SerializeField]
    private bool collapsed;
    public GameObject CollapseButton, Content;
    [SerializeField]
    private TMPro.TMP_Text text;
    [SerializeField]
    private Image Arrow;


    public bool Collapsed {
        get => collapsed;
        set => SetCollapsedState(value);
    }

    private void Start() {
        text.text = Name;
        SetCollapsedState(collapsed);
    }


    public void SetLabel(string label) {
        Name = label;
        text.text = label;
    }

    public string GetLabel() {
        return Name;
    }
    public void SetCollapsedState(bool state) {
        collapsed = state;
        Content.SetActive(!state);
        if (!Collapsed) {
            Arrow.transform.rotation = Quaternion.Euler(0, 0, 270);
        } else {
            Arrow.transform.rotation = Quaternion.Euler(0, 0, 90);
        }
    }

    public void ToggleCollapsedState() {
        SetCollapsedState(Content.activeSelf);
    }

}
