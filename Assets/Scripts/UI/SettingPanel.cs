using MixedReality.Toolkit.UX;
using UnityEngine;

namespace UI
{
    public class SettingPanel : MonoBehaviour
    {
        public GameObject panel;

        public void OnCalibrationClick() 
        {
           panel.SetActive(false);
        }
    }
}
