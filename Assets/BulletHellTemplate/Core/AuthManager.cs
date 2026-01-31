using Cysharp.Threading.Tasks;
using System;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using VContainer;

namespace BulletHellTemplate
{
    /// <summary>
    /// Handles player authentication (login / register) and
    /// shows translated status feedback based on server responses.
    /// </summary>
    [DisallowMultipleComponent]
    public sealed class AuthManager : MonoBehaviour
    {
        /*────────── UI References ──────────*/
        [Header("Input Fields")]
        public TMP_InputField loginEmailInputField;
        public TMP_InputField loginPasswordInputField;
        public TMP_InputField createEmailInputField;
        public TMP_InputField createPasswordInputField;
        public TMP_InputField createPasswordConfirmInputField;

        [Header("Screens")]
        public GameObject UILogin;
        public GameObject UICreateAccount;

        [Header("Loading UI")]
        public GameObject UILoading;
        public TextMeshProUGUI loadingText;

        [Header("Settings")]
        [Tooltip("Scene loaded after successful authentication.")]
        public string HomeSceneName = "Home";

        [Header("Status Text")]
        [Tooltip("UI text element used to show status or error messages.")]
        public TextMeshProUGUI statusText;

        /*────────── Translated Errors ──────────*/
        [Tooltip("Invalid e-mail address.")]
        public NameTranslatedByLanguage[] invalidEmailError;
        [Tooltip("Password confirmation does not match.")]
        public NameTranslatedByLanguage[] passwordMismatchError;
        [Tooltip("Password is too short.")]
        public NameTranslatedByLanguage[] passwordTooShortError;
        [Tooltip("Password is too long.")]
        public NameTranslatedByLanguage[] passwordTooLongError;
        [Tooltip("Password needs at least one uppercase letter.")]
        public NameTranslatedByLanguage[] passwordMissingUppercaseError;
        [Tooltip("Password needs at least one lowercase letter.")]
        public NameTranslatedByLanguage[] passwordMissingLowercaseError;
        [Tooltip("Password needs at least one number.")]
        public NameTranslatedByLanguage[] passwordMissingNumberError;
        [Tooltip("Password needs at least one special character.")]
        public NameTranslatedByLanguage[] passwordMissingSpecialCharError;
        [Tooltip("E-mail already registered.")]
        public NameTranslatedByLanguage[] emailAlreadyInUseError;
        [Tooltip("Credentials are incorrect.")]
        public NameTranslatedByLanguage[] invalidCredentialsError;
        [Tooltip("Generic or unknown error.")]
        public NameTranslatedByLanguage[] genericError;

        /*────────── Internals ──────────*/
        private IBackendService Backend => BackendManager.Service;

        [Inject] private BackendSettings backendSettings;

        /*────────── Unity Lifecycle ──────────*/
        private async void Start()
        {
            ToggleLoading(true, "Initializing backend…");
            await BackendManager.CheckInitialized();

            if (BackendManager.Service is not OfflineBackendService)
            {
                SecurePrefs.ClearSecurePrefsData();
            }
            else
            {               
                PlayerSave.LoadAllPurchased();
            }

            loadingText.text = LanguageManager.LanguageManager.Instance.GetTextEntryByID("loading_loading");
            Debug.Log("[AUTH MANAGER] trying auto-login…");

            if (await Backend.TryAutoLoginAsync())
            {
                SceneManager.LoadScene(HomeSceneName);
                return;
            }
            await BackendManager.Service.Logout();
            ToggleLoading(false);
            ClearStatus();
            UILogin.SetActive(true);
        }


        /*────────── UI Callbacks ──────────*/
        public void OnLoginWithEmailButton() =>
             SafeFire(async () =>
             {
                 ClearStatus();
                 ToggleLoading(true, "Verifying credentials…");

                 var res = await Backend.AccountLogin(
                     loginEmailInputField.text.Trim(),
                     loginPasswordInputField.text);

                 await HandleResult(res, "Login successful!");
             });

        public void OnPlayAsGuestButton() =>
            SafeFire(async () =>
            {
                ClearStatus();
                ToggleLoading(true, "Joining as guest…");

                var res = await Backend.PlayAsGuest();
                await HandleResult(res, "Welcome!");
            });
     
        public void OnCreateAccountButton() =>
              SafeFire(async () =>
              {
                  ClearStatus();
                  ToggleLoading(true, "Creating account…");

                  var res = await Backend.AccountRegister(
                      createEmailInputField.text.Trim(),
                      createPasswordInputField.text,
                      createPasswordConfirmInputField.text);

                  await HandleResult(res, "Account created!");
              });

        private async void SafeFire(Func<UniTask> action)
        {
            try { await action(); }
            catch (Exception ex)
            {
                ToggleLoading(false);
                statusText.text = $"<color=#ff6666>{ex.Message}</color>";
                Debug.LogException(ex);
            }
        }

        public void OnOpenCreateAccount()
        {
            ClearStatus();
            UILogin.SetActive(false);
            UICreateAccount.SetActive(true);
        }

        public void OnCancelCreateAccount()
        {
            ClearStatus();
            UICreateAccount.SetActive(false);
            UILogin.SetActive(true);
        }

        /*────────── Helpers ──────────*/
        private void ToggleLoading(bool visible, string msg = "")
        {
            UILoading.SetActive(visible);
            loadingText.text = msg;
            statusText.gameObject.SetActive(!visible);
            UILogin.SetActive(!visible && !UICreateAccount.activeSelf);
            UICreateAccount.SetActive(!visible && UICreateAccount.activeSelf);
        }

        private void ClearStatus() => statusText.text = string.Empty;

        /// <summary>
        /// Central result handler.
        /// </summary>
        private async UniTask HandleResult(RequestResult res, string okMsg)
        {            
            if (res.Success)
            {
                loadingText.text = okMsg;
                await UniTask.Delay(600);
                SceneManager.LoadScene(HomeSceneName);
            }
            else
            {
                ToggleLoading(false);
                statusText.text = $"<color=#ff6666>{ParseReason(res.Reason)}</color>";
            }
        }

        /// <summary>
        /// Converts backend error codes to translated human-readable messages.
        /// </summary>
        private string ParseReason(string reason) => reason switch
        {
            "INVALID_EMAIL" => GetTranslatedString(invalidEmailError, "Invalid email address."),
            "PASSWORD_MISMATCH" => GetTranslatedString(passwordMismatchError, "Password confirmation does not match."),
            "PASSWORD_TOO_SHORT" => GetTranslatedString(passwordTooShortError, "Password is too short."),
            "PASSWORD_TOO_LONG" => GetTranslatedString(passwordTooLongError, "Password is too long."),
            "PASSWORD_MISSING_UPPERCASE" => GetTranslatedString(passwordMissingUppercaseError, "Password needs an uppercase letter."),
            "PASSWORD_MISSING_LOWERCASE" => GetTranslatedString(passwordMissingLowercaseError, "Password needs a lowercase letter."),
            "PASSWORD_MISSING_NUMBER" => GetTranslatedString(passwordMissingNumberError, "Password needs a number."),
            "PASSWORD_MISSING_SPECIAL_CHAR" => GetTranslatedString(passwordMissingSpecialCharError, "Password needs a special character."),
            "EMAIL_ALREADY_IN_USE" => GetTranslatedString(emailAlreadyInUseError, "Email already registered."),
            "invalid_credentials" => GetTranslatedString(invalidCredentialsError, "Incorrect credentials."),
            _ => GetTranslatedString(genericError, $"Error: {reason}")
        };

        /// <summary>
        /// Returns the translation matching the current language; falls back to
        /// <paramref name="fallback"/> when no translation is found.
        /// </summary>
        private string GetTranslatedString(NameTranslatedByLanguage[] translations, string fallback)
        {
            string currentLang = LanguageManager.LanguageManager.Instance.GetCurrentLanguage();
            if (translations != null)
            {
                foreach (var trans in translations)
                    if (!string.IsNullOrEmpty(trans.LanguageId)
                        && trans.LanguageId.Equals(currentLang)
                        && !string.IsNullOrEmpty(trans.Translate))
                        return trans.Translate;
            }
            return fallback;
        }
    }
}
