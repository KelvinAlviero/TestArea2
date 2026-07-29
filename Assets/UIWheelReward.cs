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
        [SerializeField] private Image ultimateBoost;
        [SerializeField] private Image magnet;
        [SerializeField] private Image magnetImmune;
        [SerializeField] private Image dashImmune;
        [SerializeField] private Image gems;
        [SerializeField] private Image shield;
        [SerializeField] private Image coins;
        [SerializeField] private Image dash;
        [SerializeField] private TMP_Text RewardText;
        [SerializeField] private RectTransform contentRectTransform;
        [SerializeField] private bool hasMoreQueuedRewards;
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
            UIWheelSpin.DisableSpinButton();
            UIWheelSpin.EnableCloseButton();
        }

        public override void PlayShowAnimation()
        {
            // Debug.Log("PlayShowAnimation called ");
            // Ensure Init is called to register button listeners
            if (!isInitialized)
            {
                Init();
            }

            ultimateBoost.gameObject.SetActive(false); 
            magnet.gameObject.SetActive(false);
            magnetImmune.gameObject.SetActive(false);
            dashImmune.gameObject.SetActive(false);
            gems.gameObject.SetActive(false);
            shield.gameObject.SetActive(false);
            ultimateBoost.gameObject.SetActive(false); 
            coins.gameObject.SetActive(false); 
            dash.gameObject.SetActive(false); 
            

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
            closeButton.gameObject.SetActive(true);
            closeButton.interactable = true;

            switch(SpinningScript.rewardResult)
            {
            case 1:
                    Debug.Log("Reward = UltiBooster");
                    ultimateBoost.gameObject.SetActive(true); //Magnifying glass
                    RewardText.text = "x2";
                    break;
                case 2:
                    Debug.Log("Reward = Gems");
                    gems.gameObject.SetActive(true); //Magnet
                    RewardText.text = "x50";
                    break;
                case 3:
                    Debug.Log("Reward = DashImmune");
                    dashImmune.gameObject.SetActive(true); //Coins300
                    RewardText.text = "DashImmune";
                    break;
                case 4:
                    Debug.Log("Reward = Magnet");
                    magnet.gameObject.SetActive(true); //Bombs
                    RewardText.text = "Magnet";
                    break;
                case 5:
                    Debug.Log("Reward = Shield");
                    shield.gameObject.SetActive(true); //Extramoves
                    RewardText.text = "Shield";
                    break;
                case 6:
                    Debug.Log("Reward = MagnetImmune");
                    magnetImmune.gameObject.SetActive(true); //Coins 100
                    RewardText.text = "MagnetImmune";
                    break;
                case 7:
                    Debug.Log("Reward = Coins");
                    coins.gameObject.SetActive(true); //Coins 100
                    RewardText.text = "Coins x3000";
                    break;
                case 8:
                    Debug.Log("Reward = Speed");
                    dash.gameObject.SetActive(true); //Coins 100
                    RewardText.text = "Speed";
                    break;
            }
        }

        public override void PlayHideAnimation()
        {
            panelRectTransform.gameObject.SetActive(false);
            FlashImage.gameObject.SetActive(false);
            ultimateBoost.gameObject.SetActive(false); 
            magnet.gameObject.SetActive(false);
            magnetImmune.gameObject.SetActive(false);
            dashImmune.gameObject.SetActive(false);
            gems.gameObject.SetActive(false);
            shield.gameObject.SetActive(false);
            ultimateBoost.gameObject.SetActive(false); 
            coins.gameObject.SetActive(false); 
            dash.gameObject.SetActive(false); 
            closeButton.gameObject.SetActive(false);
            Fade.gameObject.SetActive(false);
            backgroundImage.gameObject.SetActive(false);
        }
    }
}