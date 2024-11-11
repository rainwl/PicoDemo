using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class TogglePanels : MonoBehaviour
{
    public List<Toggle> Toggles = new List<Toggle>();
    public List<GameObject> Panels = new List<GameObject>();

    private void Start()
    {
        OnToggleChanged(-1);
    }
    public void OnToggleChanged(int index) 
    {
        for (int i = 0; i < Panels.Count; i++)
        {
            if (i == index && Toggles[i].isOn)
            {
                Panels[i].SetActive(true);
                LayoutRebuilder.ForceRebuildLayoutImmediate(Panels[i].transform.GetComponent<RectTransform>());
            }
            else 
            {
                Panels[i].SetActive(false);
            }
        }
    }
}
