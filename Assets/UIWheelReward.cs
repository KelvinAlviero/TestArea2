using System;
using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using UnityEngine.EventSystems;
using Unity.VisualScripting;
using TMPro;
using System.Text;
using Extras;

namespace Forgehub.SpookyBubbles
{
    public class UIWheelReward : UIPage
    {
        [Header("Script References")]
        public SpinningScript SpinningScript; 
        public UIWheelSpin UIWheelSpin;
        
        [Header("References")]
        [SerializeField] private Image backgroundImage;
        [SerializeField] private Image FlashImage;
        [SerializeField] private RectTransform Fade;
        [SerializeField] private RectTransform panelRectTransform;
        [SerializeField] private Image bombReward;
        [SerializeField] private Image coinReward;
        [SerializeField] private Image coinBigReward;
        [SerializeField] private Image MagnetReward;
        [SerializeField] private Image m_GlassReward;
        [SerializeField] private Image movesReward;
        [SerializeField] private TMP_Text RewardText;
        [SerializeField] private RectTransform contentRectTransform;

        public RectTransform ContentRectTransform => contentRectTransform;

        [Header("Buttons")]
        // [SerializeField] private Button closeButton;
        [SerializeField] private Button claimButton;
        private bool isInitialized;
        

        // Start is called once before the first execution of Update after the MonoBehaviour is created

        public bool IsOpened => isPageDisplayed;
        
        
        public void Init()
        {
            Debug.Log("UIWheelReward.Init() called - registering button listeners");
            // closeButton.onClick.AddListener(OnCloseButtonClicked);
            claimButton.onClick.AddListener(OnClaimButtonClicked);
            isInitialized = true;
            
        }

        private void OnDestroy()
        {
            // closeButton.onClick.RemoveListener(OnCloseButtonClicked);
            claimButton.onClick.RemoveListener(OnClaimButtonClicked);
        }

        public void OnClaimButtonClicked()
        {
            Debug.Log("UIWheelReward: Claim Button Clicked");

            PlayHideAnimation();

            UIWheelSpin.ShowTimeText();
            UIWheelSpin.EnableCloseButton();
            UIWheelSpin.StopLightAnimation();
            UIWheelSpin.StartTimer();
            UIWheelSpin.isSpinning = false;
            UIWheelSpin.TimeText.gameObject.SetActive(true);
            Debug.Log("IsSpinning = " + UIWheelSpin.isSpinning);
            Debug.Log("UIWheelReward: Claim button clicked");
            Debug.Log("UIWheelReward: closed UI");
            Debug.Log("UIWheelReward: Enable close button in UIWheelSpin");
             
        }

        private void OnCloseButtonClicked()
        {
            Debug.Log("Close button clicked");
            PlayHideAnimation();
        }

        public override void PlayShowAnimation()
        {
            Debug.Log("PlayShowAnimation called ");
            // Ensure Init is called to register button listeners
            if (!isInitialized)
            {
                Init();
            }

            bombReward.gameObject.SetActive(false);
            coinReward.gameObject.SetActive(false);
            coinBigReward.gameObject.SetActive(false);
            MagnetReward.gameObject.SetActive(false);
            movesReward.gameObject.SetActive(false);
            m_GlassReward.gameObject.SetActive(false); 
            
            
            Debug.Log("Show rewards");

            // Set initial positions for animation
            panelRectTransform.gameObject.SetActive(true);
            FlashImage.gameObject.SetActive(true);
            Fade.gameObject.SetActive(true);
            
            // Disable raycast on Fade so it doesn't block clicks
            Image fadeImage = Fade.GetComponent<Image>();
            if (fadeImage != null)
            {
                fadeImage.raycastTarget = false;
                Debug.Log("Fade raycast target disabled");
            }

            backgroundImage.gameObject.SetActive(true);
            claimButton.gameObject.SetActive(true);
            claimButton.interactable = true;
            Debug.Log($"claimButton active: {claimButton.gameObject.activeSelf}, interactable: {claimButton.interactable}");

            switch(SpinningScript.rewardResult)
            {
            case 1:
                    Debug.Log("Reward = Magnifying Glass");
                    m_GlassReward.gameObject.SetActive(true); //Magnifying glass
                    RewardText.text = "Magnifying glass x2";
                    break;
                case 2:
                    Debug.Log("Reward = Placeholder");
                    MagnetReward.gameObject.SetActive(true); //Magnet
                    RewardText.text = "Placeholder";
                    break;
                case 3:
                    Debug.Log("Reward = 20 coins");
                    coinBigReward.gameObject.SetActive(true); //Coins300
                    RewardText.text = "Coins x20";
                    break;
                case 4:
                    Debug.Log("Reward = Placeholder");
                    bombReward.gameObject.SetActive(true); //Bombs
                    RewardText.text = "Placeholder";
                    break;
                case 5:
                    Debug.Log("Reward = Prebooster");
                    movesReward.gameObject.SetActive(true); //Extramoves
                    RewardText.text = "Prebooster";
                    break;
                case 6:
                    Debug.Log("Reward = 100 coins");
                    coinReward.gameObject.SetActive(true); //Coins 100
                    RewardText.text = "Coins x10";
                    break;
            }
        }

        public override void PlayHideAnimation()
        {
            Debug.Log("Hiding");
            panelRectTransform.gameObject.SetActive(false);
            FlashImage.gameObject.SetActive(false);
            bombReward.gameObject.SetActive(false);
            coinReward.gameObject.SetActive(false);
            coinBigReward.gameObject.SetActive(false);
            MagnetReward.gameObject.SetActive(false);
            m_GlassReward.gameObject.SetActive(false);
            movesReward.gameObject.SetActive(false);
            // closeButton.gameObject.SetActive(false);
            claimButton.gameObject.SetActive(false);
            Fade.gameObject.SetActive(false);
            backgroundImage.gameObject.SetActive(false);
        }
    }
}