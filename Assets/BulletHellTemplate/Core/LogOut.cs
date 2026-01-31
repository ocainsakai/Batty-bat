using UnityEngine;
using UnityEngine.SceneManagement;
using System.Threading.Tasks;

namespace BulletHellTemplate
{
    /// <summary>
    /// This script handles logging out from Firebase, clearing PlayerPrefs, and returning to the login scene.
    /// </summary>
    public class LogOut : MonoBehaviour
    {
        [Header("Scene to load after logout")]
        public string loginScene; 

        /// <summary>
        /// Logs out the player from Firebase, clears PlayerPrefs, and loads the login scene.
        /// </summary>
        public async void PerformLogOut()
        {
            await LogOutFromFirebase();

            // Clear PlayerPrefs data
            PlayerPrefs.DeleteAll();
            PlayerPrefs.Save();
            Debug.Log("PlayerPrefs cleared. Logging out...");

            // Load the login scene
            if (loginScene != null)
            {
                SceneManager.LoadScene(loginScene);
            }
            else
            {
                Debug.LogError("Login scene reference is not set.");
            }
        }

        /// <summary>
        /// Logs out the current user from backend asynchronously.
        /// </summary>
        private async Task LogOutFromFirebase()
        {
           await BackendManager.Service.Logout();
           await Task.Delay(100);
           LoadingManager.Singleton.LoadSceneWithLoadingScreen(loginScene);
        }
    }
}
