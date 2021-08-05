using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ActionButton : MonoBehaviour
{
    public Image Background;
    [SerializeField]
    public Button Button;
    [SerializeField]
    protected TMPro.TMP_Text text;
    private bool blinking;

    private string objectId;

    public string ObjectId {
        get => objectId;
        set => objectId = value;
    }

    // Start is called before the first frame update
    private void Awake() {
        blinking = false;
    }

    public void SetLabel(string label) {
        text.text = label;
    }

    public string GetLabel() {
        return text.text;
    }
    // Update is called once per frame
    private void Update()
    {
        if (blinking)
            Background.color = Color.Lerp(new Color(0.224f, 0.224f, 0.224f), new Color(0.063f, 0.063f, 0.063f), Mathf.PingPong(Time.time, 0.3f)*3);
    }

    public void Highlight(float time) {
        blinking = true;        
        Invoke("Disable", time);
    }

    private void Disable() {
        blinking = false;
        Background.color = new Color(0.224f, 0.224f, 0.224f);
    }

    public virtual void SetInteractable(bool interactable) {
        Button.interactable = interactable;
    }




}
