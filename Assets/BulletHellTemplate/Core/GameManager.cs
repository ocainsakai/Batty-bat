using Cysharp.Threading.Tasks;
using System.Collections;
using UnityEngine;

namespace BulletHellTemplate
{
    /// <summary>
    /// Manages the game maps, including loading scenes, spawning characters,
    /// and unlocking new maps upon game completion.
    /// </summary>
    public class GameManager : MonoBehaviour
    {
        #region Public Fields
        /// <summary>
        /// Singleton reference to the GameManager.
        /// </summary>
        public static GameManager Singleton;

        #endregion

        #region Private Fields

        /// <summary>
        /// Stores the ID of the current map being played.
        /// </summary>
        private int currentMapId;

        #endregion

        #region Unity Callbacks

        private void Awake()
        {
            // Implement singleton pattern
            if (Singleton == null)
            {
                Singleton = this;
            }
            else
            {
                Destroy(gameObject);
            }
            DontDestroyOnLoad(gameObject);
        }

        #endregion

        #region Public Methods

        /// <summary>
        /// Starts the game with the specified MapId and stores the current map ID.
        /// </summary>
        /// <param name="mapId">The ID of the map to load.</param>
        public void StartGame(int mapId)
        {
            currentMapId = mapId; // Store the ID of the current map

            foreach (var map in GameInstance.Singleton.mapInfoData)
            {
                if (map.mapId == mapId)
                {
                    bool isUnlocked = map.isUnlocked || PlayerSave.GetUnlockedMaps().Contains(map.mapId);
                    if (isUnlocked)
                    {
                        StartCoroutine(LoadSceneAndSpawnCharacter(map));
                    }
                    else
                    {
                        Debug.LogError("Map is not unlocked.");
                    }
                    return;
                }
            }
            Debug.LogError("MapId not found.");
        }

        /// <summary>
        /// Ends the game, displays a win/lose screen, and returns to the main menu.
        /// If the player wins, it checks if the current map is the latest unlocked map and unlocks the next map if true.
        /// </summary>
        /// <param name="won">Whether the player won the game.</param>
        public async UniTask EndGameAsync(bool won)
        {
            AudioManager.Singleton.StopAllAudioPlay();

#if FUSION2
            FusionLobbyManager.Instance?.EndGameSession();
#endif
            int monstersKilled = GameplayManager.Singleton.GetMonstersKilled();
            int gainedGold = GameplayManager.Singleton.GetGainGold();
            int characterId = PlayerSave.GetSelectedCharacter();

            var result = await BackendManager.Service.CompleteGameSessionAsync(new EndGameSessionData
            {
                won = won,
                mapId = currentMapId,
                characterId = characterId,
                monstersKilled = monstersKilled,
                gainedGold = gainedGold,
            });

            if (result.Success)
            {
                if (!string.IsNullOrEmpty(result.Reason))
                    Debug.Log($"EndGame Result: {result.Reason}");
            }
            else
            {
                Debug.LogWarning($"EndGame failed: {result.Reason}");
            }

            UIGameplay.Singleton.DisplayEndGameScreen(won);
        }      

        /// <summary>
        /// Retrieves the ID of the current map being played.
        /// </summary>
        /// <returns>The ID of the current map.</returns>
        public int GetCurrentMapId()
        {
            return currentMapId;
        }

        #endregion

        #region Private Methods

        /// <summary>
        /// Coroutine that loads the specified map scene asynchronously and,
        /// once the scene is fully loaded, spawns the player's character at the designated start position.
        /// Uses the LoadingManager to display the loading screen during the process.
        /// </summary>
        /// <param name="map">The map data containing the scene to be loaded.</param>
        /// <returns>An IEnumerator used for coroutine execution.</returns>
        private IEnumerator LoadSceneAndSpawnCharacter(MapInfoData map)
        {
            yield return LoadingManager.Singleton.LoadSceneWithLoadingScreen(map.scene);

            // Spawn the player's character after the scene has been loaded
            SpawnCharacterAtStartPosition(map);
        }

        /// <summary>
        /// Spawns the player's character at the starting position in the loaded scene.
        /// The character passEntryPrefab is instantiated at a default position (Vector3.zero),
        /// and the character's data is set based on the game instance.
        /// Additionally, it spawns the correct UIGameplay Canvas based on the specified platform type.
        /// </summary>
        /// <param name="map">The map data used for setting the minimap image if available.</param>
        private void SpawnCharacterAtStartPosition(MapInfoData map)
        {
            Vector3 startPosition = Vector3.zero;

            // Check if the character passEntryPrefab is set in the GameInstance
            if (GameInstance.Singleton.characterEntity != null)
            {
                // Instantiate the character at the start position
                CharacterEntity character = Instantiate(GameInstance.Singleton.characterEntity, startPosition, Quaternion.identity);

                UIGameplay uiGameplayPrefab = GameInstance.Singleton.GetUIGameplayForPlatform();
                
                
                // Get the correct UIGameplay instance based on the specified platform type
                if (uiGameplayPrefab != null)
                {
                    // Instantiate UIGameplay and store the instance
                    var uiGameplayInstance = Instantiate(uiGameplayPrefab, Vector3.zero, Quaternion.identity);

                    // Ensure the instance has the necessary components
                    if (uiGameplayInstance.minimapImage != null)
                    {
                        if (map.mapMinimapImage != null)
                        {
                            // Assign the map's sprite to the minimap
                            uiGameplayInstance.minimapImage.sprite = map.mapMinimapImage;
                        }
                        else
                        {
                            // Assign the unlockedSprite if map's sprite is null
                            uiGameplayInstance.minimapImage.sprite = uiGameplayInstance.unlockedSprite;
                        }
                    }
                    else
                    {
                        Debug.LogWarning("MinimapImage is not set in the UIGameplay passEntryPrefab.");
                    }
                }
                CharacterData selectedCharacter = GameInstance.Singleton.GetCharacterData();
                int skinIndex = PlayerSave.GetCharacterSkin(selectedCharacter.characterId);
                character.SetCharacterData(selectedCharacter,skinIndex);
            }
            else
            {
                // Log an error if the character passEntryPrefab is not set
                Debug.LogError("CharacterPrefab is not set in the GameInstance.Singleton.");
            }
        }

        /// <summary>
        /// Determines if the map is unlocked based on PlayerPrefs or default settings in the ScriptableObject.
        /// </summary>
        /// <param name="mapId">The ID of the map.</param>
        /// <returns>True if the map is unlocked, false otherwise.</returns>
        private bool IsMapUnlocked(int mapId)
        {
            // Check if the map is marked as unlocked in the editor
            foreach (var map in GameInstance.Singleton.mapInfoData)
            {
                if (map.mapId == mapId && map.isUnlocked)
                {
                    return true;
                }
            }

            // If not, check if it is unlocked in the player's saved data
            return PlayerSave.GetUnlockedMaps().Contains(mapId);
        }

        private CharacterData GetCharacterDataById(int characterId)
        {
            if (GameInstance.Singleton == null || GameInstance.Singleton.characterData == null)
                return null;
            foreach (CharacterData item in GameInstance.Singleton.characterData)
                if (item.characterId == characterId)
                    return item;
            return null;
        }

        /// <summary>
        /// Initiates the process of returning to the main menu by loading the main menu scene.
        /// </summary>
        public void ReturnToMainMenu()
        {
            StartCoroutine(LoadMainMenuAndLoadPlayerInfo());
        }

        /// <summary>
        /// Initiates the process of loading a specific scene.
        /// </summary>
        /// <param name="scene">The name of the scene to load.</param>
        public void LoadSpecificScene(string scene)
        {
            StartCoroutine(LoadAsyncSpecificScene(scene));
        }

        /// <summary>
        /// Coroutine that loads the main menu scene asynchronously and,
        /// once the scene is fully loaded, it calls the method to load the player's information.
        /// Uses the LoadingManager to display the loading screen during the process.
        /// </summary>
        /// <returns>An IEnumerator used for coroutine execution.</returns>
        private IEnumerator LoadMainMenuAndLoadPlayerInfo()
        {
            yield return LoadingManager.Singleton.LoadSceneWithLoadingScreen(GameInstance.Singleton.mainMenuScene);

            // After the scene is loaded, call the method to load player information
            UIMainMenu.Singleton.LoadPlayerInfo();
        }

        /// <summary>
        /// Coroutine that loads a specific scene asynchronously.
        /// Uses the LoadingManager to display the loading screen during the process.
        /// </summary>
        /// <param name="scene">The name of the scene to load.</param>
        /// <returns>An IEnumerator used for coroutine execution.</returns>
        private IEnumerator LoadAsyncSpecificScene(string scene)
        {
            yield return LoadingManager.Singleton.LoadSceneWithLoadingScreen(scene);
        }

        #endregion
    }
}
