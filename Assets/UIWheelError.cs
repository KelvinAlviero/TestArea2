using UnityEngine;
using UnityEngine.UI;
using TMPro;
using Extras;

namespace Forgehub.SpookyBubbles
{
    public class UIWheelError : UIPage
    {
        [Header("Script References")]
        public UIWheelSpin UIWheelSpin;

        [Header("References")]
        [SerializeField] private Image backgroundImage;
        [SerializeField] private RectTransform Fade;
        [SerializeField] private RectTransform panelRectTransform;
        [SerializeField] private TMP_Text errorText;

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
            if (!string.IsNullOrEmpty(message) && errorText != null)
                errorText.text = message;

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

            if (Fade != null)
            {
                Fade.gameObject.SetActive(true);
                Image fadeImage = Fade.GetComponent<Image>();
                if (fadeImage != null)
                    fadeImage.raycastTarget = true;
            }

            if (backgroundImage != null)
                backgroundImage.gameObject.SetActive(true);

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

            if (Fade != null)
                Fade.gameObject.SetActive(false);

            if (backgroundImage != null)
                backgroundImage.gameObject.SetActive(false);

            isPageDisplayed = false;
        }
    }
}
