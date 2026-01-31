using UnityEngine;
using UnityEngine.UI;
using TMPro;

namespace BulletHellTemplate
{
    public class RewardPopupEntry : MonoBehaviour
    {
        [Header("UI Elements")]
        public Image rewardIcon;
        public TextMeshProUGUI rewardAmount;

        public void Setup(Sprite icon, string amount)
        {
            rewardIcon.sprite = icon;
            rewardAmount.text = "x" + amount;
        }
    }
}