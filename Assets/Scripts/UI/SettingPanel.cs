using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class SettingPanel : MonoBehaviour
{
    public Toggle SettingTog;
    void Start()
    {
        
    }

    public void OnCallibrationClick() 
    {
        SettingTog.isOn = false;
    }
    void Update()
    {
        
    }
}
