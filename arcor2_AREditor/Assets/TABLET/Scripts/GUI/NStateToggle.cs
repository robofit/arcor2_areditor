using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

public class NStateToggle : MonoBehaviour {
    public List<Sprite> Images;
    public List<UnityEvent> Callbacks;
    private List<Image> circles;
    public List<string> States;
    public Image Image;
    public Image Circle;
    public Button Button;

    private int selectedIndex;

    private void Awake() {
        Debug.Assert(Images.Count == Callbacks.Count);
        Debug.Assert(Images.Count >= 2);
        selectedIndex = 0;
        Image.sprite = Images[0];
        circles = new List<Image> {
            Circle
        };
        for (int i = 1; i < Images.Count; ++i) {
            Image c = Instantiate(Circle, Circle.transform.parent).GetComponent<Image>();
            circles.Add(c);
            circles[i].transform.GetChild(0).gameObject.SetActive(false);
        }
    }

    public void OnClick() {
        if (++selectedIndex >= Images.Count)
            selectedIndex = 0;
        UpdateBtn(true);
    }

    private void UpdateBtn(bool invoke) {
        if (invoke)
            Callbacks[selectedIndex].Invoke();
        Image.sprite = Images[selectedIndex];
        for (int i = 0; i < Images.Count; ++i) {
            if (i == selectedIndex) {
                circles[i].transform.GetChild(0).gameObject.SetActive(true);
            } else {
                circles[i].transform.GetChild(0).gameObject.SetActive(false);

            }
        }
    }

    public string GetState() {
        return States[selectedIndex];
    }

    public void SetState(string newState, bool invoke) {
        int newIndex = States.IndexOf(newState);
        if (newIndex >= 0)
            selectedIndex = newIndex;
        UpdateBtn(invoke);
    }

    internal void SetInteractivity(bool interactive) {
        Button.interactable = interactive;
    }
}
