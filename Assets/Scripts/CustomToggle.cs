using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class CustomToggle : MonoBehaviour
{
    public Toggle toggle; // 绑定的Toggle组件
    public Image backgroundImage; // Toggle的背景图案

    void Start()
    {
        if (toggle == null || backgroundImage == null)
        {
            Debug.LogError("Toggle or Background Image is not assigned!");
            return;
        }

        // 监听Toggle的状态变化
        toggle.onValueChanged.AddListener(OnToggleValueChanged);
        // 初始化背景图案显示状态
        OnToggleValueChanged(toggle.isOn);
    }

    void OnToggleValueChanged(bool isOn)
    {
        // 当Toggle选中时隐藏背景图案，未选中时显示背景图案
        backgroundImage.enabled = !isOn;
    }
}