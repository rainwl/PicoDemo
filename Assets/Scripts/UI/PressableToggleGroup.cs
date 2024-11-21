using System.Collections.Generic;
using System.Linq;
using MixedReality.Toolkit.UX;
using UnityEngine;

namespace UI
{
    public class PressableToggleGroup : MonoBehaviour
    {
        public List<PressableButton> pressableToggles;
        [SerializeField] private bool m_AllowSwitchOff = false;

        public bool allowSwitchOff
        {
            get { return m_AllowSwitchOff; }
            set { m_AllowSwitchOff = value; }
        }

        protected PressableToggleGroup()
        {
        }

        protected void Start()
        {
            EnsureValidState();
        }

        protected void OnEnable()
        {
            EnsureValidState();
        }

        public void EnsureValidState()
        {
            if (!allowSwitchOff && !AnyPressableTogglesOn() && pressableToggles.Count != 0)
            {
                pressableToggles[0].ForceSetToggled(true);
            }

            IEnumerable<PressableButton> activeToggles = ActivePressableToggles();

            if (activeToggles.Count() > 1)
            {
                PressableButton firstActive = GetFirstActiveToggle();

                foreach (PressableButton toggle in activeToggles)
                {
                    if (toggle == firstActive)
                    {
                        continue;
                    }

                    toggle.ForceSetToggled(false);
                }
            }
        }

        public PressableButton GetFirstActiveToggle()
        {
            IEnumerable<PressableButton> activeToggles = ActivePressableToggles();
            return activeToggles.Count() > 0 ? activeToggles.First() : null;
        }

        public bool AnyPressableTogglesOn()
        {
            return pressableToggles.Find(x => x.IsToggled) != null;
        }

        public IEnumerable<PressableButton> ActivePressableToggles()
        {
            return pressableToggles.Where(x => x.IsToggled);
        }

        public void SetAllPressableTogglesOff(bool sendCallback = true)
        {
            bool oldAllowSwitchOff = m_AllowSwitchOff;
            m_AllowSwitchOff = true;
            if (sendCallback)
            {
                for (var i = 0; i < pressableToggles.Count; i++)
                {
                    pressableToggles[i].ForceSetToggled(false);
                }
            }
            else
            {
                for (var i = 0; i < pressableToggles.Count; i++)
                {
                    pressableToggles[i].ForceSetToggled(false);
                }
            }

            m_AllowSwitchOff = oldAllowSwitchOff;
        }
    }
}