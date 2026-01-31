using System.Collections;
using TMPro;
using UnityEngine;

namespace BulletHellTemplate
{
    /// <summary>
    /// Manages coupon redemption UI, including validating coupon codes and rewarding the player.
    /// </summary>
    public class UICoupon : MonoBehaviour
    {
        [Header("UI Components")]
        [Tooltip("Input field for entering the coupon code.")]
        public TMP_InputField couponInput;

        [Tooltip("UI text element to display the reward information.")]
        public TextMeshProUGUI rewardInfo;

        [Tooltip("UI text element to display the result message.")]
        public TextMeshProUGUI resultMessage;

        [Header("Translations")]
        [Tooltip("Fallback message when the coupon was already used.")]
        public string couponAlreadyUsed = "Coupon already used.";
        [Tooltip("Translations for 'Coupon already used'.")]
        public NameTranslatedByLanguage[] couponAlreadyUsedTranslated;

        [Tooltip("Fallback message for successful coupon claim.")]
        public string couponClaimedSuccess = "Coupon claimed successfully!";
        [Tooltip("Translations for 'Coupon claimed successfully!'.")]
        public NameTranslatedByLanguage[] couponClaimedSuccessTranslated;

        [Tooltip("Fallback message for invalid coupon code.")]
        public string couponInvalidCode = "Invalid coupon code.";
        [Tooltip("Translations for 'Invalid coupon code.'.")]
        public NameTranslatedByLanguage[] couponInvalidCodeTranslated;

        [Tooltip("Fallback format for the reward message: 'You received {0} {1}!'")]
        public string couponRewardFormat = "You received {0} {1}!";
        [Tooltip("Translations for the coupon reward format.")]
        public NameTranslatedByLanguage[] couponRewardFormatTranslated;

        private string currentLang;

        /// <summary>
        /// Subscribes to language change events.
        /// </summary>
        private void Awake()
        {
            LanguageManager.LanguageManager.onLanguageChanged += UpdateText;
        }

        /// <summary>
        /// Unsubscribes from language change events on destroy.
        /// </summary>
        private void OnDestroy()
        {
            LanguageManager.LanguageManager.onLanguageChanged -= UpdateText;
        }

        /// <summary>
        /// Initializes current language when the object is enabled.
        /// </summary>
        private void OnEnable()
        {
            currentLang = LanguageManager.LanguageManager.Instance.GetCurrentLanguage();
        }

        /// <summary>
        /// Updates the current language whenever LanguageManager triggers a language change.
        /// </summary>
        private void UpdateText()
        {
            currentLang = LanguageManager.LanguageManager.Instance.GetCurrentLanguage();
        }

        /// <summary>
        /// Opens the coupon menu, resetting input fields and messages.
        /// </summary>
        public void OnOpenCouponMenu()
        {
            couponInput.text = string.Empty;
            rewardInfo.text = string.Empty;
            resultMessage.text = string.Empty;
        }

        /// <summary>
        /// Called when the user clicks the "Claim Coupon" button.
        /// Verifies the coupon and processes the reward if valid.
        /// </summary>
        public async void OnClickClaimCoupon()
        {
            string enteredCode = couponInput.text.Trim();
            RequestResult res = await BackendManager.Service.RedeemCouponAsync(enteredCode);

            if (!res.Success)
            {
                string msg = res.Reason switch
                {
                    "0" => GetTranslatedString(couponAlreadyUsedTranslated, couponAlreadyUsed, currentLang),
                    "1" => GetTranslatedString(couponInvalidCodeTranslated, couponInvalidCode, currentLang),
                    _ => "Unknown error"
                };
                DisplayResultMessage(msg, Color.red);
                return;
            }

            string[] parts = res.Reason.Split('|');
            string currency = parts[0];
            int amount = int.Parse(parts[1]);

            string successMsg = GetTranslatedString(couponClaimedSuccessTranslated,
                                                    couponClaimedSuccess, currentLang);
            DisplayResultMessage(successMsg, Color.green);

            string rewardFmt = GetTranslatedString(couponRewardFormatTranslated,
                                                    couponRewardFormat, currentLang);
            rewardInfo.text = string.Format(rewardFmt, amount, currency);

            UIProfileMenu.Singleton.OnRedeemCoupon.Invoke();
        }

        /// <summary>
        /// Displays a result message and reward info for a limited time.
        /// </summary>
        /// <param name="message">The message to display.</param>
        /// <param name="color">The color of the message text.</param>
        private void DisplayResultMessage(string message, Color color)
        {
            resultMessage.text = message;
            resultMessage.color = color;
            StartCoroutine(ClearMessagesAfterDelay(3f));
        }

        /// <summary>
        /// Coroutine to clear the result message and reward info after a delay.
        /// </summary>
        /// <param name="delay">The delay in seconds before clearing the message and reward info.</param>
        private IEnumerator ClearMessagesAfterDelay(float delay)
        {
            yield return new WaitForSeconds(delay);
            resultMessage.text = string.Empty;
            rewardInfo.text = string.Empty;
        }

        /// <summary>
        /// Returns the translated string for the current language, or a fallback value if no translation is found.
        /// </summary>
        /// <param name="translations">Array of NameTranslatedByLanguage entries.</param>
        /// <param name="fallback">Fallback text in case the translation is not found.</param>
        /// <param name="currentLang">The current language ID.</param>
        /// <returns>A translated string if found, otherwise the fallback.</returns>
        private string GetTranslatedString(NameTranslatedByLanguage[] translations, string fallback, string currentLang)
        {
            if (translations != null)
            {
                foreach (var translation in translations)
                {
                    if (!string.IsNullOrEmpty(translation.LanguageId)
                        && translation.LanguageId.Equals(currentLang)
                        && !string.IsNullOrEmpty(translation.Translate))
                    {
                        return translation.Translate;
                    }
                }
            }
            return fallback;
        }
    }
}
