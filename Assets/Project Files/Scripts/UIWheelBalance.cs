using UnityEngine;
using UnityEngine.UI;
using TMPro;
using Extras;
using I2.Loc;

namespace Forgehub.SpookyBubbles
{
    public class UIWheelBalance : UIPage
    {
        [Header("Script References")]
        public UIWheelSpin UIWheelSpin;

        [Header("References")]
        [SerializeField] private RectTransform panelRectTransform;
        [SerializeField] private Localize errorLoc;
        [SerializeField] private Localize missionLoc;

        [Header("Buttons")]
        [SerializeField] private Button closeButton;

        private bool isInitialized;

        public bool IsOpened => isPageDisplayed;

        public void Init()
        {
            closeButton.onClick.AddListener(OnCloseButtonClicked);
            isInitialized = true;
        }

        private void OnDestroy()
        {
            if (closeButton != null)
                closeButton.onClick.RemoveListener(OnCloseButtonClicked);
        }

        public void Show(string message = null)
        {
            if (!string.IsNullOrEmpty(message) && errorLoc != null)
                errorLoc.SetTerm(message);

            PlayShowAnimation();
        }

        public void OnCloseButtonClicked()
        {
            PlayHideAnimation();

            if (UIWheelSpin != null)
            {
                UIWheelSpin.EnableSpinButton();
                UIWheelSpin.EnableCloseButton();
            }
        }

        public override void PlayShowAnimation()
        {
            if (!isInitialized)
                Init();

            if (panelRectTransform != null)
                panelRectTransform.gameObject.SetActive(true);

            if (closeButton != null)
            {
                closeButton.gameObject.SetActive(true);
                closeButton.interactable = true;
            }

            isPageDisplayed = true;
        }

        public override void PlayHideAnimation()
        {
            if (panelRectTransform != null)
                panelRectTransform.gameObject.SetActive(false);

            if (closeButton != null)
                closeButton.gameObject.SetActive(false);

            isPageDisplayed = false;
        }
    }
}
