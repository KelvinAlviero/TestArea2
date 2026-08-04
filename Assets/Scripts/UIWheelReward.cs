using UnityEngine;
using UnityEngine.UI;
using TMPro;
using Extras;

namespace Forgehub.SpookyBubbles
{
    public class UIWheelReward : UIPage
    {
        [Header("Script References")]
        private SpinningScript SpinningScript; 
        private UIWheelSpin UIWheelSpin;
        private FreeSpinChecker freeSpinChecker;
        [Header("References")]
        [SerializeField] private RectTransform panelRectTransform;
        [SerializeField] private TMP_Text RewardText;
        [SerializeField] private RectTransform contentRectTransform;
        [SerializeField] private bool hasMoreQueuedRewards;
        [SerializeField] private RewardsGetter rewardDisplay;
        public RectTransform ContentRectTransform => contentRectTransform;
        [Header("Buttons")]
        // [SerializeField] private Button closeButton;
        [SerializeField] private Button closeButton;
        private bool isInitialized;
        

        // Start is called once before the first execution of Update after the MonoBehaviour is created

        public bool IsOpened => isPageDisplayed;
        
        
        public void Init()
        {
            // closeButton.onClick.AddListener(OnCloseButtonClicked);
            closeButton.onClick.AddListener(OnCloseButtonClicked);
            isInitialized = true;
            
        }

        public void SetReward(RewardSO reward)
        {
            rewardDisplay.SetReward(reward);
        }

        private void OnDestroy()
        {
            // closeButton.onClick.RemoveListener(OnCloseButtonClicked);
            closeButton.onClick.RemoveListener(OnCloseButtonClicked);
        }

        public void OnCloseButtonClicked()
        {
            if (UIWheelSpin == null)
            {
                PlayHideAnimation();
                return;
            }

            PlayHideAnimation();

            // Keep the wheel in its current cooldown state.
            freeSpinChecker.FreeSpinCheck();
            UIWheelSpin.EnableCloseButton();
            UIWheelSpin.EnableSpinPaidButton();
            UIWheelSpin.EnableMissionButton();
        }

        public override void PlayShowAnimation()
        {
            // Debug.Log("PlayShowAnimation called ");
            // Ensure Init is called to register button listeners
            if (!isInitialized)
            {
                Init();
            }

            // Set initial positions for animation
            panelRectTransform.gameObject.SetActive(true);
            closeButton.gameObject.SetActive(true);
            closeButton.interactable = true;
        }

        public override void PlayHideAnimation()
        {
            panelRectTransform.gameObject.SetActive(false);
            closeButton.gameObject.SetActive(false);
        }
    }
}