/*
 * SensitivitySlider
 * Author: Timotej Halenár
 * Login: xhalen00
 * Bachelor's Thesis 
 * VUT FIT 2024
 * 
 * */

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
    private float sliderMin;
    private float sliderMax;
    [SerializeField] private float newMin;
    [SerializeField] private float newMax;

    // Start is called before the first frame update
    void Start()
    {
        sliderMin = slider.minValue;
        sliderMax = slider.maxValue;
        setText(slider.value);
        

        slider.onValueChanged.AddListener((t) => {
            setText(t);
        });

        
    }

    void setText(float value) {
        float newVal = newMin + ((newMax - newMin) / (sliderMax - sliderMin)) * (value - sliderMin);
        text.text = "Sensitivity: " + newVal.ToString(format);
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
