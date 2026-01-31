using UnityEngine;
using UnityEngine.SceneManagement;

namespace BulletHellTemplate
{
    /// <summary>
    /// This class handles scene switching to the home screen or any specified scene.
    /// The default scene is "Home", but it can be changed via the Inspector.
    /// The LoadingManager is used to show a loading screen during the transition.
    /// </summary>
    public class ReturnHome : MonoBehaviour
    {
        [Header("Scene Name Settings")]
        [Tooltip("Name of the scene to switch to. Defaults to 'Home'.")]
        public string sceneName = "Home"; // Editable string with default value "Home"

        /// <summary>
        /// Switches the current scene to the one specified by the sceneName variable.
        /// Uses the LoadingManager to display a loading screen during the transition.
        /// If no custom scene is set, it defaults to switching to the "Home" scene.
        /// </summary>
        public void SwitchToHomeScene()
        {
            if (GameplayManager.Singleton != null)
            {
                GameplayManager.Singleton.PauseGame();
            }
            if(GameManager.Singleton != null)
            {
                GameManager.Singleton.ReturnToMainMenu();
            }
        }
    }
}
