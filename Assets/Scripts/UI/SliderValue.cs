using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
public class SliderValue : MonoBehaviour
{
    Slider slier;
    public TextMeshProUGUI ValueText;
    
    private void Start()
    {
        slier = transform.GetComponent<Slider>();
        OnSliderChanged(slier.value);
        slier.onValueChanged.AddListener(OnSliderChanged);
    }
    void OnSliderChanged(float f) 
    {
        if(ValueText!=null)
        ValueText.text = f.ToString("F2");
    }
}
