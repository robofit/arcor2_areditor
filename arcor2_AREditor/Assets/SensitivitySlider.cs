using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class SensitivitySlider : MonoBehaviour
{
    [SerializeField] private Slider slider;
    [SerializeField] private TextMeshProUGUI text;
    [SerializeField] private string format = "0.00";

    // Start is called before the first frame update
    void Start()
    {
        slider.onValueChanged.AddListener((v) => {
            text.text = "Sensitivity: " + v.ToString(format);
        });

        
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
