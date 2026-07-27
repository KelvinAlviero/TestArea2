using UnityEngine;
using UnityEngine.UI;
using TMPro;
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
        [SerializeField] private bool hasMoreQueuedRewards;
        public RectTransform ContentRectTransform => contentRectTransform;
        [Header("Buttons")]
        // [SerializeField] private Button closeButton;
        [SerializeField] private Button claimButton;
        private bool isInitialized;
        

        // Start is called once before the first execution of Update after the MonoBehaviour is created

        public bool IsOpened => isPageDisplayed;
        
        
        public void Init()
        {
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
            // hasMoreQueuedRewards = SpinningScript.MoreSpins;

            // if (hasMoreQueuedRewards == true)
            // {
            //     Debug.Log("UIWheelReward: starting next queued spin");
            //     UIWheelSpin.OnSpinButtonClicked();
            //     UIWheelSpin.DisableSpinButton();
            //     UIWheelSpin.StopLightAnimation();
            //     PlayHideAnimation();
                
            // } 
            // else if (hasMoreQueuedRewards == false)
            // {
            // Debug.Log("No more spins");
            // Debug.Log("UIWheelReward: Claim Button Clicked");
            PlayHideAnimation();
            UIWheelSpin.EnableCloseButton();
            UIWheelSpin.EnableSpinButton();
            UIWheelSpin.StopLightAnimation();
            UIWheelSpin.isSpinning = false;
            // Debug.Log("IsSpinning = " + UIWheelSpin.isSpinning);
            
            }
        //}

        private void OnCloseButtonClicked()
        {
            Debug.Log("Close button clicked");
            PlayHideAnimation();
        }

        public override void PlayShowAnimation()
        {
            // Debug.Log("PlayShowAnimation called ");
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

            // Set initial positions for animation
            panelRectTransform.gameObject.SetActive(true);
            FlashImage.gameObject.SetActive(true);
            Fade.gameObject.SetActive(true);
            
            // Disable raycast on Fade so it doesn't block clicks
            Image fadeImage = Fade.GetComponent<Image>();
            if (fadeImage != null)
            {
                fadeImage.raycastTarget = false;
            }
            backgroundImage.gameObject.SetActive(true);
            claimButton.gameObject.SetActive(true);
            claimButton.interactable = true;

            switch(SpinningScript.rewardResult)
            {
            case 1:
                    Debug.Log("Reward = UltiBooster");
                    m_GlassReward.gameObject.SetActive(true); //Magnifying glass
                    RewardText.text = "Ultimate Booster";
                    break;
                case 2:
                    Debug.Log("Reward = Gems");
                    MagnetReward.gameObject.SetActive(true); //Magnet
                    RewardText.text = "Gems x50";
                    break;
                case 3:
                    Debug.Log("Reward = DashImmune");
                    coinBigReward.gameObject.SetActive(true); //Coins300
                    RewardText.text = "DashImmune";
                    break;
                case 4:
                    Debug.Log("Reward = Magnet");
                    bombReward.gameObject.SetActive(true); //Bombs
                    RewardText.text = "Magnet";
                    break;
                case 5:
                    Debug.Log("Reward = Shield");
                    movesReward.gameObject.SetActive(true); //Extramoves
                    RewardText.text = "Shield";
                    break;
                case 6:
                    Debug.Log("Reward = MagnetImmune");
                    coinReward.gameObject.SetActive(true); //Coins 100
                    RewardText.text = "MagnetImmune";
                    break;
                case 7:
                    Debug.Log("Reward = Coins");
                    coinReward.gameObject.SetActive(true); //Coins 100
                    RewardText.text = "Coins x3000";
                    break;
                case 8:
                    Debug.Log("Reward = Speed");
                    coinReward.gameObject.SetActive(true); //Coins 100
                    RewardText.text = "Speed";
                    break;
            }
        }

        public override void PlayHideAnimation()
        {
            // Debug.Log("Hiding");
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