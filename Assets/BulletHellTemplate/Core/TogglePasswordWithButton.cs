using UnityEngine;
using UnityEngine.UI;
using TMPro;

namespace BulletHellTemplate
{
    /// <summary>
    /// This class handles the functionality of toggling password visibility in a TMP_InputField.
    /// It allows the player to see the password when they click on a button and switch between
    /// showing and hiding the password.
    /// </summary>
    public class TogglePasswordWithButton : MonoBehaviour
    {
        [SerializeField] private TMP_InputField passwordInputField;
        [SerializeField] private TMP_InputField confirmPasswordInputField;
        [SerializeField] private Button toggleButton;
        [SerializeField] private Image eyeImage;
        [SerializeField] private Sprite eyeOpenIcon;
        [SerializeField] private Sprite eyeClosedIcon;

        private bool isPasswordVisible = false;

        /// <summary>
        /// Initialize the button icon and set the listener for toggling password visibility.
        /// </summary>
        private void Start()
        {
            // Set the initial button icon (closed eye for hidden password)
            eyeImage.sprite = eyeClosedIcon;

            // Add listener to the button click
            toggleButton.onClick.AddListener(TogglePasswordVisibility);
        }

        /// <summary>
        /// Function to toggle the visibility of the password field. 
        /// Switches between password content type and standard content type.
        /// Updates the button icon accordingly to reflect the current state.
        /// </summary>
        private void TogglePasswordVisibility()
        {
            if (isPasswordVisible)
            {
                // Hide the password by setting contentType to Password
                passwordInputField.contentType = TMP_InputField.ContentType.Password;
               if(confirmPasswordInputField) confirmPasswordInputField.contentType = TMP_InputField.ContentType.Password;
                eyeImage.sprite = eyeClosedIcon; // Update icon to closed eye
                isPasswordVisible = false;
            }
            else
            {
                // Show the password by setting contentType to Standard
                passwordInputField.contentType = TMP_InputField.ContentType.Standard;
                if (confirmPasswordInputField) confirmPasswordInputField.contentType = TMP_InputField.ContentType.Standard;
                eyeImage.sprite = eyeOpenIcon; // Update icon to open eye
                isPasswordVisible = true;
            }

            // Refresh the input field to apply the changes immediately
            passwordInputField.ForceLabelUpdate();
            if (confirmPasswordInputField) confirmPasswordInputField.ForceLabelUpdate();
        }
    }
}
