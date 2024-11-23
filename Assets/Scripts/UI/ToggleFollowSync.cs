using MixedReality.Toolkit.SpatialManipulation;
using MixedReality.Toolkit.UX;
using UnityEngine;

namespace UI
{
    public class ToggleFollowSync : MonoBehaviour
    {
        public GameObject mainPanel;
        public PressableButton button1;
        public PressableButton button2;

        private Follow _followComponent;

        private void Start()
        {
            if (!mainPanel.TryGetComponent<Follow>(out _followComponent))
            {
                Debug.LogError("The Follow component was not found on MainPanel");
                return;
            }

            _followComponent.enabled = true;

            button1.ForceSetToggled(false);
            button2.ForceSetToggled(false);

            button1.OnClicked.AddListener(() => OnButtonToggled(button1));
            button2.OnClicked.AddListener(() => OnButtonToggled(button1));
        }

        private void OnButtonToggled(PressableButton toggledButton)
        {
            toggledButton.ForceSetToggled(!toggledButton.IsToggled);

            var isAnyButtonToggled = button1.IsToggled || button2.IsToggled;

            button1.ForceSetToggled(isAnyButtonToggled);
            button2.ForceSetToggled(isAnyButtonToggled);

            _followComponent.enabled = !isAnyButtonToggled;
        }
    }
}