using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;
//using Firebase;
//using Firebase.Auth;
//using Firebase.Firestore;
using System;
using System.Linq;
using static BulletHellTemplate.PlayerSave;

namespace BulletHellTemplate
{
    public class FirebaseManager : MonoBehaviour
    {
        //[Header("Component responsible for loading and saving user data")]
        //public FirebaseAuth auth;
        //public FirebaseFirestore firestore;
        //public static FirebaseManager Singleton;

        //[Header("Daily Quest Reset Configuration")]
        //public int resetHour = 0;

        //private void Awake()
        //{
        //    if (Singleton == null)
        //    {
        //        Singleton = this;
        //    }
        //    else
        //    {
        //        Destroy(gameObject);
        //        return;
        //    }
        //    DontDestroyOnLoad(gameObject);

        //}
        //public void Start()
        //{
        //    StartInitializeFirebase();
        //}
        //public void StartInitializeFirebase()
        //{
        //    InitializeFirebase();
        //}

        //private async void InitializeFirebase()
        //{
        //    try
        //    {
        //        var dependencyStatus = await FirebaseApp.CheckAndFixDependenciesAsync();
        //        if (dependencyStatus == DependencyStatus.Available)
        //        {
        //            auth = FirebaseAuth.DefaultInstance;
        //            firestore = FirebaseFirestore.DefaultInstance;

        //            FirebaseAuthManager.Singleton.InitializeAuthBackend(auth, firestore);
        //            FirebaseSave.Singleton.InitializeFirebase(auth.CurrentUser, firestore);

        //            Debug.Log("Firebase initialized successfully.");
        //        }
        //        else
        //        {
        //            Debug.LogError("Could not resolve all Firebase dependencies: " + dependencyStatus);
        //        }
        //    }
        //    catch (Exception e)
        //    {
        //        Debug.LogError("Failed to initialize Firebase: " + e.Message);
        //    }
        //}

        ///// <summary>
        ///// Loads and synchronizes all required player data from Firestore in a single method.
        ///// If the player's document does not exist, it will create default data once.
        ///// This approach removes repeated checks and reduces extra snapshot calls.
        ///// </summary>
        ///// <param name="userId">The authenticated user's ID.</param>
        ///// <returns>A result message describing the outcome.</returns>
        //public async Task<string> LoadAndSyncPlayerData(string userId)
        //{
        //    // Single checks at the beginning
        //    if (firestore == null)
        //    {
        //        return "Failed to initialize Firestore.";
        //    }

        //    if (!MonetizationManager.Singleton.IsInitialized())
        //    {
        //        return "MonetizationManager is not initialized.";
        //    }

        //    if (auth == null || auth.CurrentUser == null)
        //    {
        //        return "No user is currently logged in.";
        //    }

        //    PlayerPrefs.DeleteAll();
        //    PlayerPrefs.Save();

        //    try
        //    {
        //        // 1) Check or create the main document
        //        DocumentReference docRef = firestore.Collection("Players").Document(userId);
        //        DocumentSnapshot snapshot = await docRef.GetSnapshotAsync();

        //        if (!snapshot.Exists)
        //        {
        //            await CreateDefaultPlayerData(docRef);
        //            snapshot = await docRef.GetSnapshotAsync();
        //            if (!snapshot.Exists)
        //            {
        //                return "Failed to create default player data.";
        //            }
        //        }

        //        // 2) Parse minimal fields in the main document
        //        Dictionary<string, object> playerData = snapshot.ToDictionary();
        //        if (playerData.TryGetValue("PlayerName", out object pName) && pName != null)
        //        {
        //            PlayerSave.SetPlayerName(pName.ToString());
        //        }
        //        if (playerData.TryGetValue("PlayerIcon", out object pIcon) && pIcon != null)
        //        {
        //            PlayerSave.SetPlayerIcon(pIcon.ToString());
        //        }
        //        if (playerData.TryGetValue("PlayerFrame", out object pFrame) && pFrame != null)
        //        {
        //            PlayerSave.SetPlayerFrame(pFrame.ToString());
        //        }
        //        if (playerData.TryGetValue("AccountLevel", out object aLevelObj) && aLevelObj != null)
        //        {
        //            // Convert the object to int properly, handling Firestore's usual 'long' storage
        //            int levelValue = Convert.ToInt32(aLevelObj);
        //            PlayerSave.SetAccountLevel(levelValue);
        //        }
        //        else
        //        {
        //            // If the field doesn't exist or is null, set a default
        //            PlayerSave.SetAccountLevel(1);
        //        }

        //        if (playerData.TryGetValue("AccountCurrentExp", out object aExpObj) && aExpObj != null)
        //        {
        //            int expValue = Convert.ToInt32(aExpObj);
        //            PlayerSave.SetAccountCurrentExp(expValue);
        //        }
        //        else
        //        {
        //            PlayerSave.SetAccountCurrentExp(0);
        //        }

        //        // 3) Run all other sub-loads in parallel to speed up the process
        //        List<Task> loadTasks = new List<Task>
        //        {
        //            LoadAndSyncCurrencies(userId),
        //            LoadPurchasedItemsAsync(userId),
        //            LoadSelectedCharacter(userId),
        //            LoadPlayerCharacterFavouriteAsync(userId),
        //            LoadAndSyncAllCharacterUpgradesAsync(userId),
        //            LoadUnlockedMapsAsync(userId),
        //            ResetDailyQuestsIfNeeded(userId),
        //            LoadQuestsAsync(userId),
        //            LoadUsedCouponsAsync(userId),
        //            CheckForSeasonEndAsync(userId),
        //            LoadBattlePassProgressAsync(),
        //            LoadClaimedRewardsFromFirebase(userId),
        //            InitializeInventoryAsync(),
        //            LoadAndSyncAllCharacterBasicInfoAsync(),
        //            LoadCharacterSlotsAsync(),
        //            LoadAndSyncAllCharacterUnlockedSkinsAsync(),

        //        };

        //        await Task.WhenAll(loadTasks);
        //        await LoadRewardsDataAsync();
        //        BackendManager.Singleton.SetInitialized();
        //        return "Player data loaded successfully.";
        //    }
        //    catch (Exception ex)
        //    {
        //        Debug.LogError($"Failed to load and synchronize player data: {ex.Message}");
        //        return $"Failed to load player data: {ex.Message}";
        //    }
        //}

        ///// <summary>
        ///// Creates a default player document if none exists.
        ///// </summary>
        //private async Task CreateDefaultPlayerData(DocumentReference docRef)
        //{
        //    try
        //    {
        //        string playerName = "GUEST-" + UnityEngine.Random.Range(100000, 999999);
        //        string defaultIconId = GameInstance.Singleton.iconItems[0].iconId;
        //        string defaultFrameId = GameInstance.Singleton.frameItems[0].frameId;

        //        Dictionary<string, object> defaultData = new Dictionary<string, object>
        //        {
        //            { "PlayerName", playerName },
        //            { "PlayerIcon", defaultIconId },
        //            { "PlayerFrame", defaultFrameId }
        //        };

        //        await docRef.SetAsync(defaultData);
        //        Debug.Log($"Created default player data for user document: {docRef.Id}");
        //    }
        //    catch (Exception ex)
        //    {
        //        Debug.LogError($"Failed to create default player data: {ex.Message}");
        //        throw;
        //    }
        //}

        ///// <summary>
        ///// Loads and synchronizes the player's currencies (parallel-ready, no repeated checks).
        ///// </summary>
        //private async Task LoadAndSyncCurrencies(string userId)
        //{
        //    try
        //    {
        //        CollectionReference currenciesCollection = firestore
        //            .Collection("Players")
        //            .Document(userId)
        //            .Collection("Currencies");

        //        foreach (Currency currency in MonetizationManager.Singleton.currencies)
        //        {
        //            DocumentReference currencyDoc = currenciesCollection.Document(currency.coinID);
        //            DocumentSnapshot snapshot = await currencyDoc.GetSnapshotAsync();

        //            if (snapshot.Exists)
        //            {
        //                int amount = snapshot.GetValue<int>("amount");
        //                MonetizationManager.SetCurrency(currency.coinID, amount);
        //            }
        //            else
        //            {
        //                Dictionary<string, object> currencyData = new Dictionary<string, object>
        //                {
        //                    { "initialAmount", currency.initialAmount },
        //                    { "amount", currency.initialAmount }
        //                };
        //                await currencyDoc.SetAsync(currencyData);
        //                MonetizationManager.SetCurrency(currency.coinID, currency.initialAmount);
        //            }
        //        }
        //        Debug.Log("Currencies loaded successfully.");
        //    }
        //    catch (Exception e)
        //    {
        //        Debug.LogError($"Failed to load currencies: {e.Message}");
        //    }
        //}


        ///// <summary>
        ///// Loads the selected character from the main document, parallel-ready.
        ///// </summary>
        //private async Task LoadSelectedCharacter(string userId)
        //{
        //    try
        //    {
        //        DocumentReference docRef = firestore.Collection("Players").Document(userId);
        //        DocumentSnapshot snapshot = await docRef.GetSnapshotAsync();

        //        if (snapshot.Exists)
        //        {
        //            int selectedCharacter = 0;
        //            if (snapshot.ContainsField("selectedCharacter"))
        //            {
        //                selectedCharacter = snapshot.GetValue<int>("selectedCharacter");
        //            }
        //            else
        //            {
        //                await docRef.UpdateAsync("selectedCharacter", 0);
        //            }
        //            PlayerSave.SetSelectedCharacter(selectedCharacter);
        //        }
        //        else
        //        {
        //            Dictionary<string, object> data = new Dictionary<string, object> { { "selectedCharacter", 0 } };
        //            await docRef.SetAsync(data);
        //            PlayerSave.SetSelectedCharacter(0);
        //        }
        //    }
        //    catch (Exception e)
        //    {
        //        Debug.LogError($"Failed to load selected character: {e.Message}");
        //    }
        //}
        //// <summary>
        ///// Loads purchased items without using 'dynamic'. Each subdocument is handled with a specific typed method.
        ///// </summary>
        //private async Task LoadPurchasedItemsAsync(string userId)
        //{
        //    try
        //    {
        //        CollectionReference docRef = firestore
        //            .Collection("Players")
        //            .Document(userId)
        //            .Collection("PurchasedItems");

        //        // Default lists
        //        var defaultCharacterList = new PurchasedCharacterList(new List<PurchasedCharacter>());
        //        var defaultIconList = new PurchasedIconList(new List<PurchasedIcon>());
        //        var defaultFrameList = new PurchasedFrameList(new List<PurchasedFrame>());
        //        var defaultShopItemList = new PurchasedShopItemList(new List<PurchasedShopItem>());

        //        // Call each "ensure + load" method separately
        //        Task<PurchasedCharacterList> taskCharacters = EnsureCharactersDocExistsAndLoad(docRef.Document("Characters"), "Characters", defaultCharacterList);
        //        Task<PurchasedIconList> taskIcons = EnsureIconsDocExistsAndLoad(docRef.Document("Icons"), "Icons", defaultIconList);
        //        Task<PurchasedFrameList> taskFrames = EnsureFramesDocExistsAndLoad(docRef.Document("Frames"), "Frames", defaultFrameList);
        //        Task<PurchasedShopItemList> taskShopItems = EnsureShopItemsDocExistsAndLoad(docRef.Document("ShopItems"), "ShopItems", defaultShopItemList);

        //        // Wait for all to complete
        //        await Task.WhenAll(taskCharacters, taskIcons, taskFrames, taskShopItems);

        //        // Retrieve the final typed results
        //        PurchasedCharacterList finalCharacters = taskCharacters.Result;
        //        PurchasedIconList finalIcons = taskIcons.Result;
        //        PurchasedFrameList finalFrames = taskFrames.Result;
        //        PurchasedShopItemList finalShopItems = taskShopItems.Result;
        //        PurchasedInventoryItemList finalItems = new PurchasedInventoryItemList(new List<PurchasedInventoryItem>());

        //        // Merge results into MonetizationManager
        //        MonetizationManager.Singleton.UpdatePurchasedItems(
        //            finalCharacters.purchasedCharacters,
        //            finalIcons.purchasedIcons,
        //            finalFrames.purchasedFrames,
        //            finalShopItems.purchasedShopItems,
        //            finalItems.purchasedInventoryItems
        //        );
        //    }
        //    catch (Exception ex)
        //    {
        //        Debug.LogError("Error loading purchased items: " + ex.Message);
        //    }
        //}

        ///// <summary>
        ///// Ensures the 'Characters' subdocument exists, creates it if not, and returns a PurchasedCharacterList.
        ///// </summary>
        //private async Task<PurchasedCharacterList> EnsureCharactersDocExistsAndLoad(
        //    DocumentReference docRef,
        //    string fieldName,
        //    PurchasedCharacterList defaultValue)
        //{
        //    var snapshot = await docRef.GetSnapshotAsync();
        //    if (!snapshot.Exists)
        //    {
        //        // Create a new document with an empty or default list
        //        await docRef.SetAsync(new Dictionary<string, object>
        //        {
        //            { fieldName, defaultValue.purchasedCharacters } // use the .purchasedCharacters list
        //        });
        //        return defaultValue;
        //    }
        //    else
        //    {
        //        // Load existing data
        //        var listData = snapshot.GetValue<List<object>>(fieldName);
        //        var typed = ConvertToList<PurchasedCharacter>(listData);
        //        return new PurchasedCharacterList(typed);
        //    }
        //}

        ///// <summary>
        ///// Ensures the 'Icons' subdocument exists, creates it if not, and returns a PurchasedIconList.
        ///// </summary>
        //private async Task<PurchasedIconList> EnsureIconsDocExistsAndLoad(
        //    DocumentReference docRef,
        //    string fieldName,
        //    PurchasedIconList defaultValue)
        //{
        //    var snapshot = await docRef.GetSnapshotAsync();
        //    if (!snapshot.Exists)
        //    {
        //        await docRef.SetAsync(new Dictionary<string, object>
        //        {
        //            { fieldName, defaultValue.purchasedIcons }
        //        });
        //        return defaultValue;
        //    }
        //    else
        //    {
        //        var listData = snapshot.GetValue<List<object>>(fieldName);
        //        var typed = ConvertToList<PurchasedIcon>(listData);
        //        return new PurchasedIconList(typed);
        //    }
        //}

        ///// <summary>
        ///// Ensures the 'Frames' subdocument exists, creates it if not, and returns a PurchasedFrameList.
        ///// </summary>
        //private async Task<PurchasedFrameList> EnsureFramesDocExistsAndLoad(
        //    DocumentReference docRef,
        //    string fieldName,
        //    PurchasedFrameList defaultValue)
        //{
        //    var snapshot = await docRef.GetSnapshotAsync();
        //    if (!snapshot.Exists)
        //    {
        //        await docRef.SetAsync(new Dictionary<string, object>
        //        {
        //            { fieldName, defaultValue.purchasedFrames }
        //        });
        //        return defaultValue;
        //    }
        //    else
        //    {
        //        var listData = snapshot.GetValue<List<object>>(fieldName);
        //        var typed = ConvertToList<PurchasedFrame>(listData);
        //        return new PurchasedFrameList(typed);
        //    }
        //}

        ///// <summary>
        ///// Ensures the 'ShopItems' subdocument exists, creates it if not, and returns a PurchasedShopItemList.
        ///// </summary>
        //private async Task<PurchasedShopItemList> EnsureShopItemsDocExistsAndLoad(
        //    DocumentReference docRef,
        //    string fieldName,
        //    PurchasedShopItemList defaultValue)
        //{
        //    var snapshot = await docRef.GetSnapshotAsync();
        //    if (!snapshot.Exists)
        //    {
        //        await docRef.SetAsync(new Dictionary<string, object>
        //        {
        //            { fieldName, defaultValue.purchasedShopItems }
        //        });
        //        return defaultValue;
        //    }
        //    else
        //    {
        //        var listData = snapshot.GetValue<List<object>>(fieldName);
        //        var typed = ConvertToList<PurchasedShopItem>(listData);
        //        return new PurchasedShopItemList(typed);
        //    }
        //}

        ///// <summary>
        ///// Loads a single character's upgrades and stores them locally.
        ///// </summary>
        //private async Task LoadCharacterUpgradesAsync(string userId, int characterId)
        //{
        //    try
        //    {
        //        DocumentReference upgradesDocRef = firestore
        //            .Collection("Players")
        //            .Document(userId)
        //            .Collection("Characters")
        //            .Document(characterId.ToString())
        //            .Collection("Upgrades")
        //            .Document("Stats");

        //        DocumentSnapshot snapshot = await upgradesDocRef.GetSnapshotAsync();
        //        if (snapshot.Exists)
        //        {
        //            Dictionary<string, object> upgradesData = snapshot.ToDictionary();
        //            foreach (var entry in upgradesData)
        //            {
        //                if (Enum.TryParse(entry.Key, out StatType statType))
        //                {
        //                    int level = Convert.ToInt32(entry.Value);
        //                    PlayerSave.SaveCharacterUpgradeLevel(characterId, statType, level);
        //                }
        //            }
        //        }
        //        else
        //        {
        //            // If no upgrades, save local defaults as 0
        //            foreach (StatType statType in Enum.GetValues(typeof(StatType)))
        //            {
        //                PlayerSave.SaveInitialCharacterUpgradeLevel(characterId, statType);
        //            }
        //        }
        //    }
        //    catch (Exception e)
        //    {
        //        Debug.LogError($"Failed to load character upgrades for {characterId}: {e.Message}");
        //    }
        //}

        ///// <summary>
        ///// Loads and saves all character upgrades in parallel for each unlocked character.
        ///// </summary>
        //private async Task LoadAndSyncAllCharacterUpgradesAsync(string userId)
        //{
        //    try
        //    {
        //        List<Task> upgradeTasks = new List<Task>();
        //        foreach (CharacterData character in GameInstance.Singleton.characterData)
        //        {
        //            if (character.CheckUnlocked || MonetizationManager.Singleton.IsCharacterPurchased(character.characterId.ToString()))
        //            {
        //                upgradeTasks.Add(LoadCharacterUpgradesAsync(userId, character.characterId));
        //            }
        //        }
        //        await Task.WhenAll(upgradeTasks);
        //    }
        //    catch (Exception e)
        //    {
        //        Debug.LogError("Failed to load all character upgrades: " + e.Message);
        //    }
        //}

        ///// <summary>
        ///// Loads the unlocked skins for a specific character from Firebase Firestore and saves them locally.
        ///// </summary>
        ///// <param name="userId">The user's ID.</param>
        ///// <param name="characterId">The ID of the character.</param>
        //public async Task LoadCharacterUnlockedSkinsAsync(string userId, int characterId)
        //{
        //    try
        //    {
        //        DocumentReference skinsDocRef = firestore
        //            .Collection("Players")
        //            .Document(userId)
        //            .Collection("Characters")
        //            .Document(characterId.ToString())
        //            .Collection("UnlockedSkins")
        //            .Document("Skins");

        //        DocumentSnapshot snapshot = await skinsDocRef.GetSnapshotAsync();
        //        if (snapshot.Exists)
        //        {
        //            Dictionary<string, object> skinsData = snapshot.ToDictionary();
        //            if (skinsData.ContainsKey("skins"))
        //            {
        //                List<int> unlockedSkins = new List<int>();
        //                // Firestore returns numeric array elements as long.
        //                if (skinsData["skins"] is IList<object> skinsList)
        //                {
        //                    foreach (var skin in skinsList)
        //                    {
        //                        if (skin is long l)
        //                        {
        //                            unlockedSkins.Add((int)l);
        //                        }
        //                        else if (skin is int i)
        //                        {
        //                            unlockedSkins.Add(i);
        //                        }
        //                    }
        //                }
        //                // Save unlocked skins locally using PlayerSave.
        //                PlayerSave.SaveCharacterUnlockedSkins(characterId, unlockedSkins);
        //                Debug.Log($"Unlocked skins for character {characterId} loaded and saved locally.");
        //            }
        //            else
        //            {
        //                Debug.LogWarning($"No 'skins' field found for character {characterId}.");
        //            }
        //        }
        //        else
        //        {
        //            Debug.LogWarning($"No unlocked skins document found for character {characterId}.");
        //        }
        //    }
        //    catch (Exception e)
        //    {
        //        Debug.LogError($"Failed to load unlocked skins for character {characterId}: {e.Message}");
        //    }
        //}

        ///// <summary>
        ///// Loads and synchronizes unlocked skins for all unlocked or purchased characters.
        ///// </summary>
        //public async Task LoadAndSyncAllCharacterUnlockedSkinsAsync()
        //{
        //    if (auth.CurrentUser == null)
        //    {
        //        Debug.LogError("No user is logged in.");
        //        return;
        //    }

        //    try
        //    {
        //        List<Task> tasks = new List<Task>();
        //        foreach (CharacterData character in GameInstance.Singleton.characterData)
        //        {
        //            if (character.CheckUnlocked || MonetizationManager.Singleton.IsCharacterPurchased(character.characterId.ToString()))
        //            {
        //                tasks.Add(LoadCharacterUnlockedSkinsAsync(auth.CurrentUser.UserId, character.characterId));
        //            }
        //        }
        //        await Task.WhenAll(tasks);
        //    }
        //    catch (Exception e)
        //    {
        //        Debug.LogError("Failed to load all character unlocked skins: " + e.Message);
        //    }
        //}

        ///// <summary>
        ///// Loads unlocked maps, clearing local data first.
        ///// </summary>
        //private async Task LoadUnlockedMapsAsync(string userId)
        //{
        //    try
        //    {
        //        DocumentReference mapsDocRef = firestore
        //            .Collection("Players")
        //            .Document(userId)
        //            .Collection("Progress")
        //            .Document("UnlockedMaps");

        //        DocumentSnapshot snapshot = await mapsDocRef.GetSnapshotAsync();
        //        if (snapshot.Exists)
        //        {
        //            Dictionary<string, object> mapsData = snapshot.ToDictionary();
        //            List<int> unlockedMapIds = new List<int>();
        //            foreach (var entry in mapsData)
        //            {
        //                if (int.TryParse(entry.Key, out int mapId))
        //                {
        //                    unlockedMapIds.Add(mapId);
        //                }
        //            }
        //            PlayerSave.SetUnlockedMaps(unlockedMapIds);
        //        }
        //    }
        //    catch (Exception e)
        //    {
        //        Debug.LogError("Failed to load unlocked maps: " + e.Message);
        //    }
        //}

        ///// <summary>
        ///// Loads quests from the "Quests" document and saves them locally.
        ///// </summary>
        //private async Task LoadQuestsAsync(string userId)
        //{
        //    try
        //    {
        //        DocumentReference questsDocRef = firestore
        //            .Collection("Players")
        //            .Document(userId)
        //            .Collection("Progress")
        //            .Document("Quests");

        //        DocumentSnapshot snapshot = await questsDocRef.GetSnapshotAsync();
        //        if (snapshot.Exists)
        //        {
        //            Dictionary<string, object> questsData = snapshot.ToDictionary();
        //            foreach (var entry in questsData)
        //            {
        //                if (entry.Key.StartsWith("Complete"))
        //                {
        //                    string[] parts = entry.Key.Split(' ');
        //                    if (parts.Length == 2 && int.TryParse(parts[1], out int questId))
        //                    {
        //                        PlayerSave.SaveQuestCompletion(questId);
        //                    }
        //                }
        //                else if (int.TryParse(entry.Key, out int questId))
        //                {
        //                    int progress = Convert.ToInt32(entry.Value);
        //                    PlayerSave.SaveQuestProgress(questId, progress);
        //                }
        //            }
        //        }
        //    }
        //    catch (Exception e)
        //    {
        //        Debug.LogError("Failed to load quests: " + e.Message);
        //    }
        //}

        ///// <summary>
        ///// Checks if daily quests need resetting based on server timestamp.
        ///// </summary>
        //private async Task ResetDailyQuestsIfNeeded(string userId)
        //{
        //    try
        //    {
        //        DocumentReference serverTimeRef = firestore
        //            .Collection("Players")
        //            .Document(userId)
        //            .Collection("ServerTime")
        //            .Document("CurrentTime");

        //        await serverTimeRef.SetAsync(new Dictionary<string, object> { { "timestamp", FieldValue.ServerTimestamp } });
        //        DocumentSnapshot serverTimeSnapshot = await serverTimeRef.GetSnapshotAsync();
        //        Timestamp serverTimestamp = serverTimeSnapshot.GetValue<Timestamp>("timestamp");

        //        DateTime serverDateTime = serverTimestamp.ToDateTime();
        //        DateTime resetTimeToday = serverDateTime.Date.AddHours(resetHour);

        //        await ResetDailyQuestProgressInFirebase(userId, serverDateTime, resetTimeToday);
        //    }
        //    catch (Exception e)
        //    {
        //        Debug.LogError("Failed to reset daily quests: " + e.Message);
        //    }
        //}

        //private async Task ResetDailyQuestProgressInFirebase(string userId, DateTime serverDateTime, DateTime resetTimeToday)
        //{
        //    try
        //    {
        //        DocumentReference questsDocRef = firestore
        //            .Collection("Players")
        //            .Document(userId)
        //            .Collection("Progress")
        //            .Document("Quests");

        //        DocumentSnapshot snapshot = await questsDocRef.GetSnapshotAsync();
        //        if (snapshot.Exists)
        //        {
        //            Dictionary<string, object> questsData = snapshot.ToDictionary();
        //            foreach (var entry in questsData)
        //            {
        //                if (entry.Key.StartsWith("Complete "))
        //                {
        //                    string[] parts = entry.Key.Split(' ');
        //                    if (parts.Length == 2 && int.TryParse(parts[1], out int questId))
        //                    {
        //                        QuestItem questItem = GameInstance.Singleton.questData.FirstOrDefault(q => q.questId == questId);
        //                        if (questItem != null && questItem.questType == QuestType.Daily)
        //                        {
        //                            string completionTimeKey = $"Complete {questId}_Timestamp";
        //                            if (questsData.TryGetValue(completionTimeKey, out object timestampValue) && timestampValue is Timestamp completionTimestamp)
        //                            {
        //                                DateTime completionTime = completionTimestamp.ToDateTime();
        //                                if (completionTime < resetTimeToday)
        //                                {
        //                                    await questsDocRef.UpdateAsync(new Dictionary<string, object> { { questId.ToString(), 0 } });
        //                                    await questsDocRef.UpdateAsync(new Dictionary<string, object> { { "Complete " + questId, FieldValue.Delete } });
        //                                    await questsDocRef.UpdateAsync(new Dictionary<string, object> { { "Complete " + questId + "_Timestamp", FieldValue.Delete } });

        //                                    PlayerPrefs.DeleteKey(completionTimeKey);
        //                                    Debug.Log($"Quest {questId} has been reset.");
        //                                }
        //                            }
        //                        }
        //                    }
        //                }
        //            }
        //        }
        //    }
        //    catch (Exception e)
        //    {
        //        Debug.LogError("Failed to reset daily quest progress: " + e.Message);
        //    }
        //}

        ///// <summary>
        ///// Loads both Daily Rewards and New Player Rewards data from Firebase,
        ///// retrieves the server time, and saves it locally in PlayerSave.
        ///// If the documents do not exist, they are created with default values.
        ///// </summary>
        //public async Task LoadRewardsDataAsync()
        //{
        //    FirebaseUser user = auth.CurrentUser;
        //    if (user == null)
        //    {
        //        Debug.LogError("Cannot load rewards data. No authenticated user found.");
        //        return;
        //    }

        //    DateTime serverDateTime = await FetchServerTimeAsync(user.UserId);
        //    if (serverDateTime == DateTime.MinValue)
        //    {
        //        serverDateTime = DateTime.Now;
        //    }

        //    DailyRewardsData dailyData = await LoadDailyRewardsFromFirebase(user.UserId, serverDateTime);
        //    NewPlayerRewardsData newPlayerData = await LoadNewPlayerRewardsFromFirebase(user.UserId);

        //    PlayerSave.SetDailyRewardsLocal(dailyData);
        //    PlayerSave.SetNewPlayerRewardsLocal(newPlayerData);

        //    DateTime resetTimeToday = serverDateTime.Date.AddHours(resetHour);
        //    DateTime nextReset = (serverDateTime >= resetTimeToday)
        //        ? resetTimeToday.AddDays(1)
        //        : resetTimeToday;

        //    PlayerSave.SetNextDailyReset(nextReset);
        //    Debug.Log($"Daily & New Player rewards loaded. Next reset: {nextReset}");
        //}

        ///// <summary>
        ///// Fetches the current server time from Firestore and returns it as a DateTime.
        ///// </summary>
        //private async Task<DateTime> FetchServerTimeAsync(string userId)
        //{
        //    try
        //    {
        //        DocumentReference serverTimeRef = firestore
        //            .Collection("Players")
        //            .Document(userId)
        //            .Collection("ServerTime")
        //            .Document("CurrentTime");

        //        await serverTimeRef.SetAsync(new Dictionary<string, object>
        //        {
        //            { "timestamp", FieldValue.ServerTimestamp }
        //        });

        //        DocumentSnapshot snapshot = await serverTimeRef.GetSnapshotAsync();
        //        Timestamp serverTimestamp = snapshot.GetValue<Timestamp>("timestamp");
        //        return serverTimestamp.ToDateTime();
        //    }
        //    catch (Exception e)
        //    {
        //        Debug.LogError($"Failed to fetch server time: {e.Message}");
        //        return DateTime.MinValue;
        //    }
        //}

        ///// <summary>
        ///// Loads the daily reward data from Firebase and returns a structured object.
        ///// If the document does not exist, creates a new one with the current server date.
        ///// </summary>
        //private async Task<DailyRewardsData> LoadDailyRewardsFromFirebase(string userId, DateTime serverDateTime)
        //{
        //    DailyRewardsData result = new DailyRewardsData();

        //    try
        //    {
        //        CollectionReference dailyRewardsCollection = firestore
        //            .Collection("Players")
        //            .Document(userId)
        //            .Collection("DailyRewards");

        //        DocumentReference docRef = dailyRewardsCollection.Document("RewardData");
        //        DocumentSnapshot snapshot = await docRef.GetSnapshotAsync();

        //        if (snapshot.Exists)
        //        {
        //            Dictionary<string, object> data = snapshot.ToDictionary();

        //            if (data.ContainsKey("firstClaimDate"))
        //            {
        //                Timestamp ts = (Timestamp)data["firstClaimDate"];
        //                result.firstClaimDate = ts.ToDateTime();
        //            }
        //            else
        //            {
        //                result.firstClaimDate = serverDateTime.Date;
        //            }

        //            if (data.ContainsKey("claimedRewards"))
        //            {
        //                result.claimedRewards = new List<int>();
        //                foreach (var obj in (List<object>)data["claimedRewards"])
        //                {
        //                    result.claimedRewards.Add(Convert.ToInt32(obj));
        //                }
        //            }
        //            else
        //            {
        //                result.claimedRewards = new List<int>();
        //            }
        //        }
        //        else
        //        {
        //            result.firstClaimDate = serverDateTime.Date;
        //            result.claimedRewards = new List<int>();

        //            Dictionary<string, object> defaultData = new Dictionary<string, object>
        //            {
        //                { "firstClaimDate", Timestamp.FromDateTime(serverDateTime.Date.ToUniversalTime()) },
        //                { "claimedRewards", new List<int>() }
        //            };

        //            await docRef.SetAsync(defaultData);
        //            Debug.Log("Created new DailyRewards document with default data.");
        //        }
        //    }
        //    catch (Exception e)
        //    {
        //        Debug.LogError($"Error loading daily rewards: {e.Message}");
        //    }

        //    return result;
        //}

        ///// <summary>
        ///// Loads the new player reward data from Firebase and returns a structured object.
        ///// </summary>
        //private async Task<NewPlayerRewardsData> LoadNewPlayerRewardsFromFirebase(string userId)
        //{
        //    NewPlayerRewardsData result = new NewPlayerRewardsData();

        //    try
        //    {
        //        DocumentReference docRef = firestore
        //            .Collection("Players")
        //            .Document(userId)
        //            .Collection("NewPlayerRewards")
        //            .Document("RewardData");

        //        DocumentSnapshot snapshot = await docRef.GetSnapshotAsync();
        //        if (snapshot.Exists)
        //        {
        //            Dictionary<string, object> data = snapshot.ToDictionary();

        //            if (data.ContainsKey("accountCreationDate"))
        //            {
        //                Timestamp ts = (Timestamp)data["accountCreationDate"];
        //                result.accountCreationDate = ts.ToDateTime();
        //            }
        //            else
        //            {
        //                result.accountCreationDate = DateTime.Now.Date;
        //            }

        //            if (data.ContainsKey("claimedRewards"))
        //            {
        //                result.claimedRewards = new List<int>();
        //                foreach (var obj in (List<object>)data["claimedRewards"])
        //                {
        //                    result.claimedRewards.Add(Convert.ToInt32(obj));
        //                }
        //            }
        //            else
        //            {
        //                result.claimedRewards = new List<int>();
        //            }
        //        }
        //        else
        //        {
        //            result.accountCreationDate = DateTime.Now.Date;
        //            result.claimedRewards = new List<int>();
        //        }
        //    }
        //    catch (Exception e)
        //    {
        //        Debug.LogError($"Error loading new player rewards: {e.Message}");
        //    }

        //    return result;
        //}


        ///// <summary>
        ///// Loads the player's used coupons.
        ///// </summary>
        //private async Task LoadUsedCouponsAsync(string userId)
        //{
        //    try
        //    {
        //        DocumentReference couponsDocRef = firestore
        //            .Collection("Players")
        //            .Document(userId)
        //            .Collection("Progress")
        //            .Document("UsedCoupons");

        //        DocumentSnapshot snapshot = await couponsDocRef.GetSnapshotAsync();
        //        if (snapshot.Exists)
        //        {
        //            Dictionary<string, object> couponsData = snapshot.ToDictionary();
        //            List<string> usedCoupons = new List<string>();

        //            foreach (var entry in couponsData)
        //            {
        //                usedCoupons.Add(entry.Key);
        //            }
        //            PlayerSave.SaveUsedCoupons(usedCoupons);
        //        }
        //        else
        //        {
        //            PlayerSave.SaveUsedCoupons(new List<string>());
        //        }
        //    }
        //    catch (Exception e)
        //    {
        //        Debug.LogError($"Failed to load used coupons: {e.Message}");
        //    }
        //}

        ///// <summary>
        ///// Retrieves the top 30 players with the highest scores.
        ///// </summary>
        ///// <returns>A list of dictionaries containing player data.</returns>
        //public async Task<List<Dictionary<string, object>>> GetTopPlayersAsync()
        //{
        //    List<Dictionary<string, object>> topPlayers = new List<Dictionary<string, object>>();

        //    try
        //    {

        //        CollectionReference playersRef = firestore.Collection("Players");

        //        Query query = playersRef.OrderByDescending("score").Limit(30);
        //        QuerySnapshot querySnapshot = await query.GetSnapshotAsync();

        //        foreach (DocumentSnapshot document in querySnapshot.Documents)
        //        {
        //            topPlayers.Add(document.ToDictionary());
        //        }

        //        Debug.Log("Top players retrieved successfully.");
        //    }
        //    catch (System.Exception e)
        //    {
        //        Debug.LogError("Error retrieving top players: " + e.Message);
        //    }

        //    return topPlayers;
        //}

        ///// <summary>
        ///// Loads the player's favorite character from the main document, parallel-ready.
        ///// </summary>
        //private async Task LoadPlayerCharacterFavouriteAsync(string userId)
        //{
        //    try
        //    {
        //        DocumentReference docRef = firestore.Collection("Players").Document(userId);
        //        DocumentSnapshot snapshot = await docRef.GetSnapshotAsync();

        //        if (snapshot.Exists && snapshot.ContainsField("PlayerCharacterFavourite"))
        //        {
        //            int favouriteCharacter = snapshot.GetValue<int>("PlayerCharacterFavourite");
        //            PlayerSave.SetFavouriteCharacter(favouriteCharacter);
        //        }
        //        else
        //        {
        //            await docRef.UpdateAsync("PlayerCharacterFavourite", 0);
        //            PlayerSave.SetFavouriteCharacter(0);
        //        }
        //    }
        //    catch (Exception e)
        //    {
        //        Debug.LogError($"Failed to load favourite character: {e.Message}");
        //    }
        //}

        ///// <summary>
        ///// Retrieves the player's current rank based on their score.
        ///// </summary>
        ///// <returns>The player's rank as an integer.</returns>
        //public async Task<int> GetPlayerRankAsync()
        //{
        //    FirebaseUser user = auth.CurrentUser;
        //    if (user == null)
        //    {
        //        Debug.LogError("No user is logged in.");
        //        return -1; // Return an invalid rank if no user is logged in
        //    }

        //    try
        //    {
        //        // Retrieve all players ordered by score
        //        QuerySnapshot allPlayersSnapshot = await firestore.Collection("Players")
        //            .OrderByDescending("score")
        //            .GetSnapshotAsync();

        //        int rank = 1;

        //        // Iterate through the players and find the rank of the current player
        //        foreach (DocumentSnapshot document in allPlayersSnapshot.Documents)
        //        {
        //            if (document.Id == user.UserId)
        //            {
        //                return rank;
        //            }
        //            rank++;
        //        }

        //        // If the player's ID was not found
        //        return -1;
        //    }
        //    catch (Exception e)
        //    {
        //        Debug.LogError("Failed to get player rank: " + e.Message);
        //        return -1;
        //    }
        //}

        ///// <summary>
        ///// Loads the Battle Pass progress from Firebase and checks if the player's season is up-to-date.
        ///// If the player's season data is missing, it will save the current season from the global settings.
        ///// </summary>
        //public async Task LoadBattlePassProgressAsync()
        //{
        //    FirebaseUser user = auth.CurrentUser;
        //    if (user == null)
        //    {
        //        Debug.LogError("No user is logged in.");
        //        return;
        //    }

        //    try
        //    {
        //        // Reference to the Battle Pass progress and season documents
        //        DocumentReference battlePassDocRef = firestore
        //            .Collection("Players")
        //            .Document(user.UserId)
        //            .Collection("Progress")
        //            .Document("BattlePass");

        //        DocumentReference seasonDocRef = firestore
        //            .Collection("Players")
        //            .Document(user.UserId)
        //            .Collection("Progress")
        //            .Document("Season");

        //        // Get snapshots of the Battle Pass progress and season data
        //        DocumentSnapshot battlePassSnapshot = await battlePassDocRef.GetSnapshotAsync();
        //        DocumentSnapshot seasonSnapshot = await seasonDocRef.GetSnapshotAsync();

        //        if (battlePassSnapshot.Exists)
        //        {
        //            // Load Battle Pass XP, Level, and Unlock Status
        //            int currentXP = battlePassSnapshot.ContainsField("CurrentXP") ? battlePassSnapshot.GetValue<int>("CurrentXP") : 0;
        //            int currentLevel = battlePassSnapshot.ContainsField("CurrentLevel") ? battlePassSnapshot.GetValue<int>("CurrentLevel") : 1;
        //            bool isUnlocked = battlePassSnapshot.ContainsField("IsUnlocked") && battlePassSnapshot.GetValue<bool>("IsUnlocked");

        //            // Set the Battle Pass manager values
        //            BattlePassManager.Singleton.currentXP = currentXP;
        //            BattlePassManager.Singleton.currentLevel = currentLevel;
        //            BattlePassManager.Singleton.xpForNextLevel = BattlePassManager.Singleton.CalculateXPForNextLevel(currentLevel);

        //            // Season management
        //            int currentSeason;
        //            if (!seasonSnapshot.Exists || !seasonSnapshot.ContainsField("CurrentSeason"))
        //            {
        //                currentSeason = await GetCurrentSeasonAsync();
        //                await seasonDocRef.SetAsync(new Dictionary<string, object> { { "CurrentSeason", currentSeason } });
        //                Debug.Log($"Player's current season saved: {currentSeason}");
        //            }
        //            else
        //            {
        //                currentSeason = seasonSnapshot.GetValue<int>("CurrentSeason");
        //            }

        //            // If the Battle Pass is unlocked, store that information locally
        //            if (isUnlocked)
        //            {
        //                PlayerPrefs.SetInt(BattlePassManager.Singleton.playerPrefsPassUnlockedKey, 1);
        //            }

        //            // Save XP and Level to PlayerPrefs
        //            PlayerPrefs.SetInt(BattlePassManager.Singleton.playerPrefsXPKey, currentXP);
        //            PlayerPrefs.SetInt(BattlePassManager.Singleton.playerPrefsLevelKey, currentLevel);

        //            // Set the current season in the BattlePassManager
        //            BattlePassManager.SetPlayerBattlePassSeason(currentSeason);

        //            // Save PlayerPrefs changes
        //            PlayerPrefs.Save();

        //            Debug.Log("Battle Pass progress loaded from Firebase successfully.");
        //        }

        //    }
        //    catch (Exception e)
        //    {
        //        Debug.LogError("Failed to load Battle Pass progress: " + e.Message);
        //    }
        //}
        ///// <summary>
        ///// Loads claimed rewards for the player's Battle Pass.
        ///// </summary>
        //private async Task LoadClaimedRewardsFromFirebase(string userId)
        //{
        //    try
        //    {
        //        DocumentReference rewardsDocRef = firestore
        //            .Collection("Players")
        //            .Document(userId)
        //            .Collection("Progress")
        //            .Document("BattlePassRewards");

        //        DocumentSnapshot snapshot = await rewardsDocRef.GetSnapshotAsync();
        //        if (snapshot.Exists)
        //        {
        //            Dictionary<string, object> rewardData = snapshot.ToDictionary();
        //            foreach (KeyValuePair<string, object> entry in rewardData)
        //            {
        //                string rewardId = entry.Key;
        //                bool isClaimed = (bool)entry.Value;
        //                if (isClaimed)
        //                {
        //                    BattlePassManager.Singleton.MarkRewardAsClaimed(rewardId);
        //                }
        //            }
        //            PlayerPrefs.Save();
        //        }
        //    }
        //    catch (Exception e)
        //    {
        //        Debug.LogError($"Failed to load claimed rewards: {e.Message}");
        //    }
        //}
        ///// <summary>
        ///// Resets Battle Pass progress in Firebase if you want to call it automatically.
        ///// </summary>
        //public async Task ResetBattlePassProgressAsync()
        //{
        //    string userId = auth.CurrentUser.UserId;
        //    try
        //    {
        //        DocumentReference battlePassDocRef = firestore
        //            .Collection("Players")
        //            .Document(userId)
        //            .Collection("Progress")
        //            .Document("BattlePass");

        //        await battlePassDocRef.DeleteAsync();

        //        DocumentReference battlePassRewardsDocRef = firestore
        //            .Collection("Players")
        //            .Document(userId)
        //            .Collection("Progress")
        //            .Document("BattlePassRewards");

        //        await battlePassRewardsDocRef.DeleteAsync();

        //        int currentSeason = await GetCurrentSeasonAsync();
        //        DocumentReference seasonDocRef = firestore
        //            .Collection("Players")
        //            .Document(userId)
        //            .Collection("Progress")
        //            .Document("Season");

        //        await seasonDocRef.SetAsync(new Dictionary<string, object> { { "CurrentSeason", currentSeason } });
        //        BattlePassManager.SetPlayerBattlePassSeason(currentSeason);
        //    }
        //    catch (Exception e)
        //    {
        //        Debug.LogError($"Failed to reset Battle Pass: {e.Message}");
        //    }
        //}


        ///// <summary>
        ///// Retrieves the current Battle Pass season from the Firestore database.
        ///// If the season document or field is missing, it returns a default value of 1.
        ///// </summary>
        ///// <returns>The current Battle Pass season as an integer.</returns>
        //public async Task<int> GetCurrentSeasonAsync()
        //{
        //    try
        //    {
        //        // Reference to the document that contains the current season information
        //        DocumentReference seasonDocRef = firestore.Collection("BattlePass").Document("SeasonInfo");
        //        DocumentSnapshot snapshot = await seasonDocRef.GetSnapshotAsync();

        //        if (snapshot.Exists)
        //        {
        //            // Try to retrieve the value of the 'Season' field
        //            if (snapshot.TryGetValue<int>("Season", out int currentSeason))
        //            {
        //                Debug.Log($"Current Season from Firestore: {currentSeason}");
        //                return currentSeason;
        //            }
        //            else
        //            {
        //                Debug.LogWarning("Season field is missing in Firestore document.");
        //                return 1; // Default value if the 'Season' field is missing
        //            }
        //        }
        //        else
        //        {
        //            Debug.LogWarning("Season document does not exist in Firestore.");
        //            return 1; // Default value if the document does not exist
        //        }
        //    }
        //    catch (System.Exception e)
        //    {
        //        Debug.LogError($"Failed to get current season: {e.Message}");
        //        return 1; // Default value in case of an error
        //    }
        //}
        ///// <summary>
        ///// Checks if the season has ended (parallel call if you want). 
        ///// </summary>
        //private async Task<bool> CheckForSeasonEndAsync(string userId)
        //{
        //    // You can remove user checks, as we did them in the main method
        //    try
        //    {
        //        DocumentReference seasonDocRef = firestore
        //            .Collection("BattlePass")
        //            .Document("SeasonInfo");

        //        DocumentSnapshot snapshot = await seasonDocRef.GetSnapshotAsync();
        //        if (snapshot.Exists && snapshot.ContainsField("StartSeason"))
        //        {
        //            Timestamp startSeasonTimestamp = snapshot.GetValue<Timestamp>("StartSeason");
        //            DateTime startSeasonDate = startSeasonTimestamp.ToDateTime();
        //            int seasonLengthInDays = BattlePassManager.Singleton.SeasonLengthInDays;
        //            DateTime seasonEndDate = startSeasonDate.AddDays(seasonLengthInDays);

        //            BattlePassManager.Singleton.SaveRemainingSeasonTimeLocally(startSeasonDate);

        //            if (DateTime.UtcNow >= seasonEndDate)
        //            {
        //                Debug.Log("The current season has ended.");
        //                BattlePassManager.SetPlayerBattlePassSeasonEnded();
        //                return true;
        //            }
        //        }
        //        return false;
        //    }
        //    catch (Exception e)
        //    {
        //        Debug.LogError($"Failed to check for season end: {e.Message}");
        //        return false;
        //    }
        //}

        ///// <summary>
        ///// Loads basic character info (skin, level, experience, mastery) from Firestore
        ///// and applies to local PlayerSave.
        ///// </summary>
        ///// <param name="characterId">ID of the character.</param>
        //public async Task LoadCharacterBasicInfoAsync(int characterId)
        //{
        //    if (auth.CurrentUser == null)
        //    {
        //        Debug.LogError("No user is logged in.");
        //        return;
        //    }

        //    try
        //    {
        //        string userId = auth.CurrentUser.UserId;
        //        DocumentReference charDocRef = firestore
        //            .Collection("Players")
        //            .Document(userId)
        //            .Collection("Characters")
        //            .Document(characterId.ToString());

        //        DocumentSnapshot snapshot = await charDocRef.GetSnapshotAsync();
        //        if (!snapshot.Exists)
        //        {
        //            Debug.LogWarning($"No character info found in Firestore for CharacterId {characterId}.");
        //            return;
        //        }

        //        // Apply to local PlayerSave if fields exist
        //        if (snapshot.ContainsField("CharacterSelectedSkin"))
        //        {
        //            int skinIndex = snapshot.GetValue<int>("CharacterSelectedSkin");
        //            PlayerSave.SetCharacterSkin(characterId, skinIndex);
        //        }
        //        if (snapshot.ContainsField("CharacterLevel"))
        //        {
        //            int level = snapshot.GetValue<int>("CharacterLevel");
        //            PlayerSave.SetCharacterLevel(characterId, level);
        //        }
        //        if (snapshot.ContainsField("CharacterCurrentExp"))
        //        {
        //            int currentExp = snapshot.GetValue<int>("CharacterCurrentExp");
        //            PlayerSave.SetCharacterCurrentExp(characterId, currentExp);
        //        }
        //        if (snapshot.ContainsField("CharacterMasteryLevel"))
        //        {
        //            int masteryLevel = snapshot.GetValue<int>("CharacterMasteryLevel");
        //            PlayerSave.SetCharacterMasteryLevel(characterId, masteryLevel);
        //        }
        //        if (snapshot.ContainsField("CharacterCurrentMasteryExp"))
        //        {
        //            int masteryExp = snapshot.GetValue<int>("CharacterCurrentMasteryExp");
        //            PlayerSave.SetCharacterCurrentMasteryExp(characterId, masteryExp);
        //        }

        //        Debug.Log($"Character basic info loaded for CharacterId {characterId}.");
        //    }
        //    catch (Exception e)
        //    {
        //        Debug.LogError($"Failed to load character basic info for {characterId}: {e.Message}");
        //    }
        //}

        //public async Task LoadAndSyncAllCharacterBasicInfoAsync()
        //{
        //    if (auth.CurrentUser == null)
        //    {
        //        Debug.LogError("No user is logged in.");
        //        return;
        //    }

        //    try
        //    {
        //        List<Task> tasks = new List<Task>();
        //        foreach (CharacterData character in GameInstance.Singleton.characterData)
        //        {
        //            if (character.CheckUnlocked || MonetizationManager.Singleton.IsCharacterPurchased(character.characterId.ToString()))
        //            {
        //                tasks.Add(LoadCharacterBasicInfoAsync(character.characterId));
        //            }
        //        }
        //        await Task.WhenAll(tasks);
        //    }
        //    catch (Exception e)
        //    {
        //        Debug.LogError("Failed to load all character info: " + e.Message);
        //    }
        //}

        ///// <summary>
        ///// Loads the item documents for each known slot of each character in GameInstance.Singleton.characterData.
        ///// For each 'slotName' in charData.itemSlots and charData.runeSlots,
        ///// fetches the subcollection path: /Players/{userId}/Characters/{charId}/{slotName}.
        ///// Then loads all docs found (usually only 1 doc, if you equip 1 item por slot).
        ///// </summary>
        //public async Task LoadCharacterSlotsAsync()
        //{
        //    if (auth.CurrentUser == null)
        //    {
        //        Debug.LogError("No user is logged in.");
        //        return;
        //    }
        //    if (GameInstance.Singleton == null || GameInstance.Singleton.characterData == null)
        //    {
        //        Debug.LogError("GameInstance or characterData is null. Cannot load character slots.");
        //        return;
        //    }

        //    string userId = auth.CurrentUser.UserId;

        //    try
        //    {
        //        foreach (var charData in GameInstance.Singleton.characterData)
        //        {
        //            int charId = charData.characterId;
        //            if (charData.itemSlots != null)
        //            {
        //                foreach (string slotName in charData.itemSlots)
        //                {
        //                    await LoadSlotDocsForCharacter(userId, charId, slotName);
        //                }
        //            }
        //            if (charData.runeSlots != null)
        //            {
        //                foreach (string slotName in charData.runeSlots)
        //                {
        //                    await LoadSlotDocsForCharacter(userId, charId, slotName);
        //                }
        //            }

        //            Debug.Log($"[LoadCharacterSlotsAsync] Finished loading slots for Character {charId}.");
        //        }

        //        Debug.Log("[LoadCharacterSlotsAsync] Finished loading slots for ALL characters.");
        //    }
        //    catch (Exception e)
        //    {
        //        Debug.LogError($"[LoadCharacterSlotsAsync] Failed: {e.Message}");
        //    }
        //}

        ///// <summary>
        ///// Loads *all* documents in the slot subcollection of a single character.
        ///// Path: /Players/{userId}/Characters/{charId}/{slotName}
        ///// Each doc is typically a single item (uniqueItemGuid).
        ///// </summary>
        //private async Task LoadSlotDocsForCharacter(string userId, int charId, string slotName)
        //{
        //    CollectionReference slotCol = firestore
        //        .Collection("Players")
        //        .Document(userId)
        //        .Collection("Characters")
        //        .Document(charId.ToString())
        //        .Collection(slotName);

        //    QuerySnapshot snapshot = await slotCol.GetSnapshotAsync();
        //    foreach (DocumentSnapshot doc in snapshot.Documents)
        //    {
        //        if (!doc.Exists)
        //            continue;

        //        string uniqueGuid = doc.Id;
        //        string itemId = doc.ContainsField("ItemId") ? doc.GetValue<string>("ItemId") : "";
        //        int itemLevel = doc.ContainsField("ItemLevel") ? doc.GetValue<int>("ItemLevel") : 1;

        //        PlayerSave.SetCharacterSlotItem(charId, slotName, uniqueGuid);

        //        PlayerPrefs.SetInt($"{charId}_{slotName}_level", itemLevel);

        //        Debug.Log($"[LoadSlotDocsForCharacter] Loaded item '{itemId}' (GUID={uniqueGuid}) at slot '{slotName}', level={itemLevel}, for Char {charId}.");
        //    }
        //}

        ///// <summary>
        ///// Loads all purchased items from Firestore into local MonetizationManager,
        ///// then checks each InventoryItem in GameInstance. If 'isUnlocked' is true and
        ///// no item with the same itemId exists, purchases it to create a new GUID.
        ///// </summary>
        //public async Task InitializeInventoryAsync()
        //{
        //    if (auth.CurrentUser == null)
        //    {
        //        Debug.LogError("No user is logged in.");
        //        return;
        //    }
        //    List<PurchasedInventoryItem> remotePurchasedItems = await LoadAllPurchasedInventoryItemsAsync();
        //    var existingCharacters = MonetizationManager.Singleton.GetPurchasedCharacters();
        //    var existingIcons = MonetizationManager.Singleton.GetPurchasedIcons();
        //    var existingFrames = MonetizationManager.Singleton.GetPurchasedFrames();
        //    var existingShopItems = MonetizationManager.Singleton.GetPurchasedShopItems();

        //    MonetizationManager.Singleton.UpdatePurchasedItems(
        //        existingCharacters,
        //        existingIcons,
        //        existingFrames,
        //        existingShopItems,
        //        remotePurchasedItems
        //    );
        //    bool needSaveItem = false;
        //    foreach (var soItem in GameInstance.Singleton.inventoryItems)
        //    {
        //        if (soItem.isUnlocked)
        //        {
        //            bool alreadyOwned = MonetizationManager.Singleton
        //                .GetPurchasedInventoryItems()
        //                .Exists(pi => pi.itemId == soItem.itemId);

        //            if (!alreadyOwned)
        //            {
        //                MonetizationManager.Singleton.PurchaseInventoryItemNoImmediateSave(soItem.itemId);
        //                Debug.Log($"Created new purchased item for {soItem.itemId} (isUnlocked).");
        //                needSaveItem = true;
        //            }
        //        }
        //    }
        //    MonetizationManager.Singleton.SavePurchasedItems(needSaveItem);
        //}


        ///// <summary>
        ///// Loads all purchased inventory items from Firestore into a List, ensuring that the parent document exists.
        ///// </summary>
        //public async Task<List<PurchasedInventoryItem>> LoadAllPurchasedInventoryItemsAsync()
        //{
        //    List<PurchasedInventoryItem> result = new List<PurchasedInventoryItem>();

        //    try
        //    {
        //        string userId = auth.CurrentUser.UserId;
        //        // Reference to the "Items" document under PurchasedItems
        //        DocumentReference itemsDocRef = firestore
        //            .Collection("Players")
        //            .Document(userId)
        //            .Collection("PurchasedItems")
        //            .Document("Items");

        //        // Check if the "Items" document exists; if not, create it.
        //        DocumentSnapshot itemsDocSnapshot = await itemsDocRef.GetSnapshotAsync();
        //        if (!itemsDocSnapshot.Exists)
        //        {
        //            await itemsDocRef.SetAsync(new Dictionary<string, object>());
        //            Debug.Log("Created the 'Items' document as it did not exist.");
        //        }

        //        // Get the collection reference for the "List" subcollection
        //        CollectionReference itemsColRef = itemsDocRef.Collection("List");

        //        QuerySnapshot snapshot = await itemsColRef.GetSnapshotAsync();
        //        foreach (DocumentSnapshot doc in snapshot.Documents)
        //        {
        //            if (!doc.Exists)
        //                continue;

        //            string uniqueGuid = doc.Id;
        //            string itemId = doc.ContainsField("itemId") ? doc.GetValue<string>("itemId") : "";
        //            int itemLevel = doc.ContainsField("itemLevel") ? doc.GetValue<int>("itemLevel") : 0;

        //            Dictionary<int, int> upgrades = new Dictionary<int, int>();
        //            if (doc.ContainsField("itemUpgrades"))
        //            {
        //                Dictionary<string, object> upgradesData = doc.GetValue<Dictionary<string, object>>("itemUpgrades");
        //                foreach (var kvp in upgradesData)
        //                {
        //                    if (int.TryParse(kvp.Key, out int upgradeIndex))
        //                    {
        //                        int upgradeValue = Convert.ToInt32(kvp.Value);
        //                        upgrades[upgradeIndex] = upgradeValue;
        //                    }
        //                }
        //            }

        //            PurchasedInventoryItem pi = new PurchasedInventoryItem
        //            {
        //                uniqueItemGuid = uniqueGuid,
        //                itemId = itemId,
        //                itemLevel = itemLevel,
        //                upgrades = upgrades
        //            };
        //            result.Add(pi);
        //        }

        //        Debug.Log($"Loaded {result.Count} purchased inventory items from Firestore.");
        //    }
        //    catch (Exception e)
        //    {
        //        Debug.LogError($"Failed to load purchased inventory items: {e.Message}");
        //    }

        //    return result;
        //}


        ///// <summary>
        ///// Converts a list of generic objects to a list of a specific type using JsonUtility.
        ///// </summary>
        //private List<T> ConvertToList<T>(List<object> data)
        //{
        //    var list = new List<T>();
        //    if (data == null) return list;

        //    foreach (var item in data)
        //    {
        //        try
        //        {
        //            list.Add(JsonUtility.FromJson<T>(item.ToString()));
        //        }
        //        catch (Exception ex)
        //        {
        //            Debug.LogError($"Error converting item: {ex.Message}");
        //        }
        //    }
        //    return list;
        //}
    }
}
