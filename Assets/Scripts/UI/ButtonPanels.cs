using MixedReality.Toolkit.UX;
using UnityEngine;
using UnityEngine.UI;

namespace UI
{
    public class ButtonPanels : MonoBehaviour
    {
        public PressableButton[] buttons;
        public GameObject[] panels;

        private void Start()
        {
            for (var i = 0; i < buttons.Length; i++)
            {
                var index = i;
                buttons[i].OnClicked.AddListener(() => TogglePanel(index));
            }
        }

        private void TogglePanel(int index)
        {
            var isActive = panels[index].activeSelf;
            foreach (var panel in panels)
            {
                panel.SetActive(false);
            }

            panels[index].SetActive(!isActive);
        }
    }
}