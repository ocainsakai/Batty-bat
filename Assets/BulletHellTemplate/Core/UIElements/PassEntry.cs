using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace BulletHellTemplate
{
    public class PassEntry : MonoBehaviour
    {
        [Header("UI Elements")]
        public TextMeshProUGUI title;
        public TextMeshProUGUI description;
        public Image icon;
        public Image freeFrame;
        public Image paidFrame;
        public Image lockedImage;
        public Image isClaimedImage; // Image to indicate if the reward is claimed
        public GameObject buttonClaim; // Button for claiming rewards

        [Header("Level Pass")]
        public GameObject levelEXPContainer;
        public Image rewardAvaliable;
        public Image currentLevelProgressBar;

        private string passID;
        private bool isPaid;
        private bool isUnlocked;
        private bool isClaimed;
        private int requiredLevel; // The level required to claim this reward
        private BattlePassItem item;
        public void SetPassItemInfo(BattlePassItem _item,string _passID, string _title,string _description, Sprite _icon, bool _isPaid, bool _isUnlocked, bool _isClaimed, int _index)
        {
            item = _item;
            passID = _passID;
            title.text = _title;
            description.text = _description;
            icon.sprite = _icon;
            isPaid = _isPaid;
            isUnlocked = _isUnlocked;
            isClaimed = _isClaimed;
            requiredLevel = _index + 1; 

            freeFrame.gameObject.SetActive(!isPaid);
            paidFrame.gameObject.SetActive(isPaid);
            lockedImage.gameObject.SetActive(!isUnlocked);
            isClaimedImage.gameObject.SetActive(isClaimed); // Show claimed image if the reward is already claimed

            // Update progress bar and available reward indicator
            UpdateProgressBarAndRewardIndicator();

            UpdateClaimButtonVisibility();
        }
        private void UpdateProgressBarAndRewardIndicator()
        {
            int currentLevel = BattlePassManager.Singleton.currentLevel;          
            if (currentLevel >= requiredLevel)
            {
                currentLevelProgressBar.fillAmount = 1f; 
            }
            else if (currentLevel + 1 == requiredLevel)
            {
                float xpPercentage = (float)BattlePassManager.Singleton.currentXP / BattlePassManager.Singleton.xpForNextLevel;
                currentLevelProgressBar.fillAmount = xpPercentage; 
            }
            else
            {
                currentLevelProgressBar.fillAmount = 0f; 
            }
           
            bool canClaimReward = !isClaimed &&
                                  isUnlocked &&
                                  currentLevel >= requiredLevel &&
                                  (!isPaid || (isPaid && PlayerSave.CheckBattlePassPremiumUnlocked()));

            rewardAvaliable.gameObject.SetActive(canClaimReward);
           
            if (requiredLevel == GameInstance.Singleton.maxLevelPass)
            {
                levelEXPContainer.SetActive(false);
            }
        }
        public void UpdateProgressBar()
        {
            UpdateProgressBarAndRewardIndicator();
        }

        private void UpdateClaimButtonVisibility()
        {
            int currentLevel = BattlePassManager.Singleton.currentLevel;

            // Logic to determine if the claim button should be shown
            if (isClaimed)
            {
                buttonClaim.SetActive(false);
            }
            else if (isUnlocked && currentLevel >= requiredLevel && (!isPaid || (isPaid && PlayerSave.CheckBattlePassPremiumUnlocked())))
            {
                buttonClaim.SetActive(true);
            }
            else
            {
                buttonClaim.SetActive(false);
            }
        }

        public async void OnClickClaimRewards()
        {
            if (isClaimed)
            {
                Debug.LogWarning("Reward already claimed.");
                return;
            }

            int currentLevel = BattlePassManager.Singleton.currentLevel;
            if (currentLevel < requiredLevel)
            {
                Debug.LogWarning($"Cannot claim reward. Required level: {requiredLevel}");
                return;
            }

            if (isUnlocked)
            {
                UIBattlePass.Singleton.RefreshXP(null, EventArgs.Empty);
                await BattlePassManager.Singleton.ClaimRewardAsync(item);
                isClaimed = true;
                isClaimedImage.gameObject.SetActive(true);
                buttonClaim.SetActive(false); 
                UIBattlePass.Singleton.ShowClaimPopup(title.text, icon.sprite, description.text);
            }
            else if (isPaid)
            {
                UIBattlePass.Singleton.ShowBuyPopup();
            }
            else
            {
                Debug.Log("This reward is locked.");
            }
        }
    }
}
