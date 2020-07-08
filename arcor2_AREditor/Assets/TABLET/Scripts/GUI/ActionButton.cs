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
    private TMPro.TMP_Text text;
    // Start is called before the first frame update
    private void Awake() {
        enabled = false;
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
        Background.color = Color.Lerp(new Color(0.353f, 0.651f, 0.945f), new Color(0.063f, 0.216f, 0.369f), Mathf.PingPong(Time.time, 0.3f)*3);
    }

    public void Highlight(float time) {
        enabled = true;        
        Invoke("Disable", time);
    }

    private void Disable() {
        enabled = false;
        Background.color = new Color(0.353f, 0.651f, 0.945f);
    }
}
