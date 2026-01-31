//using Firebase.Auth;
//using Firebase.Firestore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using UnityEngine;
using static BulletHellTemplate.PlayerSave;

namespace BulletHellTemplate
{
    public class FirebaseSave : MonoBehaviour
    {
        //public static FirebaseSave Singleton;
        //public FirebaseUser user;
        //public FirebaseFirestore firestore;

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

        //public void InitializeFirebase(FirebaseUser _user, FirebaseFirestore _firestore)
        //{
        //    user = _user;
        //    firestore = _firestore;
        //}

        ///// <summary>
        ///// Updates the player's icon in Firestore.
        ///// </summary>
        ///// <param name="newIconId">The new icon ID to set.</param>
        //public async Task UpdatePlayerIcon(string newIconId)
        //{
        //    if (user != null)
        //    {
        //        try
        //        {
        //            DocumentReference docRef = firestore.Collection("Players").Document(user.UserId);
        //            await docRef.UpdateAsync("PlayerIcon", newIconId);
        //            Debug.Log("Player icon updated successfully.");
        //        }
        //        catch (Exception e)
        //        {
        //            Debug.LogError("Failed to update player icon: " + e.Message);
        //        }
        //    }
        //    else
        //    {
        //        Debug.LogError("No user is logged in.");
        //    }
        //}

        ///// <summary>
        ///// Updates the player's frame in Firestore.
        ///// </summary>
        ///// <param name="newFrameId">The new frame ID to set.</param>
        //public async Task UpdatePlayerFrame(string newFrameId)
        //{
        //    if (user != null)
        //    {
        //        try
        //        {
        //            DocumentReference docRef = firestore.Collection("Players").Document(user.UserId);
        //            await docRef.UpdateAsync("PlayerFrame", newFrameId);
        //            Debug.Log("Player frame updated successfully.");
        //        }
        //        catch (Exception e)
        //        {
        //            Debug.LogError("Failed to update player frame: " + e.Message);
        //        }
        //    }
        //    else
        //    {
        //        Debug.LogError("No user is logged in.");
        //    }
        //}

        ///// <summary>
        ///// Updates the player's account level and current experience in Firestore.
        ///// </summary>
        ///// <param name="level">The new account level to set.</param>
        ///// <param name="currentExp">The new current experience to set.</param>
        //public async Task UpdatePlayerAccountLevel(int level, int currentExp)
        //{
        //    if (user != null)
        //    {
        //        try
        //        {
        //            DocumentReference docRef = firestore.Collection("Players").Document(user.UserId);
        //            var data = new Dictionary<string, object>
        //        {
        //            { "AccountLevel", level },
        //            { "AccountCurrentExp", currentExp }
        //        };
        //            await docRef.SetAsync(data, SetOptions.MergeAll);
        //            Debug.Log("Account info updated successfully.");
        //        }
        //        catch (Exception e)
        //        {
        //            Debug.LogError("Failed to update account info: " + e.Message);
        //        }
        //    }
        //    else
        //    {
        //        Debug.LogError("No user is logged in.");
        //    }
        //}

        ///// <summary>
        ///// Updates the player's name in Firestore.
        ///// </summary>
        ///// <param name="newName">The new player name to set.</param>
        //public async Task UpdatePlayerName(string newName)
        //{
        //    if (user != null)
        //    {
        //        try
        //        {
        //            // Validate the new player name
        //            if (string.IsNullOrWhiteSpace(newName) || newName.Length < 3 || newName.Length > 14)
        //            {
        //                Debug.LogError("Player name must be between 3 and 14 characters.");
        //                return;
        //            }

        //            // Check if the new player name already exists in the database
        //            bool nameExists = await CheckIfNameExists(newName);
        //            if (nameExists)
        //            {
        //                Debug.LogError("The player name already exists.");
        //                return;
        //            }

        //            // Update the player's name in Firestore
        //            DocumentReference docRef = firestore.Collection("Players").Document(user.UserId);
        //            await docRef.UpdateAsync("PlayerName", newName);

        //            // Update the player's name locally
        //            PlayerSave.SetPlayerName(newName);
        //            Debug.Log("Player name updated successfully.");


        //        }
        //        catch (Exception e)
        //        {
        //            Debug.LogError("Failed to update player name: " + e.Message);
        //        }
        //    }
        //    else
        //    {
        //        Debug.LogError("No user is logged in.");
        //    }
        //}

        ///// <summary>
        ///// Checks if a player name already exists in the Firestore database.
        ///// </summary>
        ///// <param name="playerName">The name to check.</param>
        ///// <returns>True if the name exists; otherwise, false.</returns>
        //public async Task<bool> CheckIfNameExists(string playerName)
        //{
        //    try
        //    {
        //        // Reference to the "Players" collection
        //        CollectionReference playersCollection = firestore.Collection("Players");

        //        // Create a query to find documents where "PlayerName" equals the provided name
        //        Query query = playersCollection.WhereEqualTo("PlayerName", playerName);

        //        // Execute the query and get a snapshot of the results
        //        QuerySnapshot querySnapshot = await query.GetSnapshotAsync();

        //        // Check if any document exists in the result
        //        return querySnapshot.Count > 0;
        //    }
        //    catch (Exception e)
        //    {
        //        Debug.LogError("Failed to check if name exists: " + e.Message);
        //        return false;
        //    }
        //}

        ///// <summary>
        ///// Updates the amount of a specific currency in Firestore.
        ///// </summary>
        ///// <param name="currencyId">The ID of the currency to update.</param>
        ///// <param name="newAmount">The new amount to set for the currency.</param>
        //public async Task UpdateCurrencies(string currencyId, int newAmount)
        //{
        //    if (user != null)
        //    {
        //        try
        //        {
        //            DocumentReference currencyDocRef = firestore
        //                .Collection("Players")
        //                .Document(user.UserId)
        //                .Collection("Currencies")
        //                .Document(currencyId);
        //            await currencyDocRef.UpdateAsync("amount", newAmount);

        //            Debug.Log($"Currency {currencyId} updated successfully to {newAmount}.");
        //        }
        //        catch (Exception e)
        //        {
        //            Debug.LogError($"Failed to update currency {currencyId}: " + e.Message);
        //        }
        //    }
        //    else
        //    {
        //        Debug.LogError("No user is logged in.");
        //    }
        //}

        ///// <summary>
        ///// Updates the selected character for the logged-in user in Firestore.
        ///// If the field doesn't exist, it creates it.
        ///// </summary>
        ///// <param name="characterId">The ID of the selected character to save.</param>
        //public async Task UpdateSelectedCharacter(int characterId)
        //{
        //    if (user != null)
        //    {
        //        try
        //        {
        //            DocumentReference docRef = firestore.Collection("Players").Document(user.UserId);

        //            await docRef.UpdateAsync("selectedCharacter", characterId);
        //            Debug.Log($"Selected character updated successfully: {characterId}");
        //        }
        //        catch (Exception e)
        //        {

        //            if (e.Message.Contains("No document to update"))
        //            {
        //                Debug.LogWarning("Document does not exist, creating field instead.");

        //                Dictionary<string, object> data = new Dictionary<string, object>
        //        {
        //            { "selectedCharacter", characterId }
        //        };
        //                await firestore.Collection("Players").Document(user.UserId).SetAsync(data, SetOptions.MergeAll);
        //                Debug.Log($"Selected character created successfully: {characterId}");
        //            }
        //            else
        //            {
        //                Debug.LogError($"Failed to update or create selected character: {e.Message}");
        //            }
        //        }
        //    }
        //    else
        //    {
        //        Debug.LogError("No user is logged in.");
        //    }
        //}

        ///// <summary>
        ///// Updates the list of purchased items for the currently logged-in user in Firestore.
        ///// </summary>
        ///// <returns>A task representing the asynchronous operation.</returns>
        //public async Task SavePurchasedItemsAsync()
        //{
        //    try
        //    {
        //        if (user == null)
        //        {
        //            Debug.LogError("No user is currently logged in.");
        //            return;
        //        }
        //        string userId = user.UserId;
        //        var characters = MonetizationManager.Singleton.GetPurchasedCharacters();
        //        var icons = MonetizationManager.Singleton.GetPurchasedIcons();
        //        var frames = MonetizationManager.Singleton.GetPurchasedFrames();
        //        var shopItems = MonetizationManager.Singleton.GetPurchasedShopItems();

        //        // Convert lists to the format suitable for Firestore
        //        var characterData = characters.Select(c => JsonUtility.ToJson(c)).ToList();
        //        var iconData = icons.Select(i => JsonUtility.ToJson(i)).ToList();
        //        var frameData = frames.Select(f => JsonUtility.ToJson(f)).ToList();
        //        var shopItemData = shopItems.Select(s => JsonUtility.ToJson(s)).ToList();

        //        var db = FirebaseFirestore.DefaultInstance;
        //        var batch = db.StartBatch();

        //        // Save purchased characters
        //        var characterDocRef = db.Collection("Players").Document(userId).Collection("PurchasedItems").Document("Characters");
        //        batch.Set(characterDocRef, new { Characters = characterData });

        //        // Save purchased icons
        //        var iconDocRef = db.Collection("Players").Document(userId).Collection("PurchasedItems").Document("Icons");
        //        batch.Set(iconDocRef, new { Icons = iconData });

        //        // Save purchased frames
        //        var frameDocRef = db.Collection("Players").Document(userId).Collection("PurchasedItems").Document("Frames");
        //        batch.Set(frameDocRef, new { Frames = frameData });

        //        // Save purchased shop items
        //        var shopItemDocRef = db.Collection("Players").Document(userId).Collection("PurchasedItems").Document("ShopItems");
        //        batch.Set(shopItemDocRef, new { ShopItems = shopItemData });

        //        // Commit the batch operation
        //        await batch.CommitAsync();
        //        Debug.Log("Purchased items saved successfully.");
        //    }
        //    catch (Exception ex)
        //    {
        //        Debug.LogError("Error saving purchased items: " + ex.Message);
        //    }
        //}

        ///// <summary>
        ///// Saves the upgrades of a specific character to Firebase Firestore.
        ///// </summary>
        ///// <param name="characterId">The ID of the character.</param>
        ///// <param name="upgrades">The dictionary containing upgrade data.</param>
        //public async Task SaveCharacterUpgradesAsync(string characterId, Dictionary<StatType, int> upgrades)
        //{
        //    if (user != null)
        //    {
        //        try
        //        {
        //            DocumentReference upgradesDocRef = firestore
        //                .Collection("Players")
        //                .Document(user.UserId)
        //                .Collection("Characters")
        //                .Document(characterId)
        //                .Collection("Upgrades")
        //                .Document("Stats");

        //            Dictionary<string, object> upgradesData = new Dictionary<string, object>();
        //            foreach (var upgrade in upgrades)
        //            {
        //                upgradesData[upgrade.Key.ToString()] = upgrade.Value;
        //            }

        //            await upgradesDocRef.SetAsync(upgradesData);
        //            Debug.Log($"Character upgrades for {characterId} saved successfully.");
        //        }
        //        catch (Exception e)
        //        {
        //            Debug.LogError($"Failed to save character upgrades for {characterId}: " + e.Message);
        //        }
        //    }
        //    else
        //    {
        //        Debug.LogError("No user is logged in.");
        //    }
        //}

        ///// <summary>
        ///// Saves the unlocked skins for a specific character to Firebase Firestore.
        ///// </summary>
        ///// <param name="characterId">The ID of the character as a string.</param>
        ///// <param name="unlockedSkins">List of unlocked skin indices.</param>
        //public async Task SaveCharacterUnlockedSkinsAsync(string characterId, List<int> unlockedSkins)
        //{
        //    if (user != null)
        //    {
        //        try
        //        {
        //            DocumentReference skinsDocRef = firestore
        //                .Collection("Players")
        //                .Document(user.UserId)
        //                .Collection("Characters")
        //                .Document(characterId)
        //                .Collection("UnlockedSkins")
        //                .Document("Skins");

        //            Dictionary<string, object> skinsData = new Dictionary<string, object>
        //            {
        //                { "skins", unlockedSkins }
        //            };

        //            await skinsDocRef.SetAsync(skinsData);
        //            Debug.Log($"Character unlocked skins for {characterId} saved successfully.");
        //        }
        //        catch (Exception e)
        //        {
        //            Debug.LogError($"Failed to save unlocked skins for {characterId}: {e.Message}");
        //        }
        //    }
        //    else
        //    {
        //        Debug.LogError("No user is logged in.");
        //    }
        //}

        ///// <summary>
        ///// Saves the unlocked maps for the player to Firebase Firestore.
        ///// </summary>
        ///// <returns>A task representing the asynchronous operation.</returns>
        //public async Task SaveUnlockedMapsAsync()
        //{
        //    if (user == null)
        //    {
        //        Debug.LogError("No user is logged in.");
        //        return;
        //    }

        //    try
        //    {
        //        List<int> unlockedMapIds = PlayerSave.GetUnlockedMaps();
        //        Dictionary<string, object> mapsData = new Dictionary<string, object>();

        //        foreach (int mapId in unlockedMapIds)
        //        {
        //            mapsData[mapId.ToString()] = true;
        //        }

        //        DocumentReference mapsDocRef = firestore
        //            .Collection("Players")
        //            .Document(user.UserId)
        //            .Collection("Progress")
        //            .Document("UnlockedMaps");

        //        await mapsDocRef.SetAsync(mapsData);
        //        Debug.Log("Unlocked maps saved to Firebase successfully.");
        //    }
        //    catch (Exception e)
        //    {
        //        Debug.LogError("Failed to save unlocked maps: " + e.Message);
        //    }
        //}

        ///// <summary>
        ///// Saves a used coupon to Firebase Firestore.
        ///// </summary>
        ///// <param name="couponId">The unique ID of the coupon.</param>
        ///// <returns>A task representing the asynchronous operation.</returns>
        //public async Task SaveUsedCouponAsync(string couponId)
        //{
        //    if (user == null)
        //    {
        //        Debug.LogError("No user is logged in.");
        //        return;
        //    }

        //    try
        //    {
        //        DocumentReference couponsDocRef = firestore
        //            .Collection("Players")
        //            .Document(user.UserId)
        //            .Collection("Progress")
        //            .Document("UsedCoupons");
        //        DocumentSnapshot snapshot = await couponsDocRef.GetSnapshotAsync();

        //        if (!snapshot.Exists)
        //        {
        //            // If the document does not exist, create it with the initial coupon
        //            await couponsDocRef.SetAsync(new Dictionary<string, object>
        //    {
        //        { couponId, true }
        //    });
        //            Debug.Log($"Coupon {couponId} created and saved to Firebase successfully.");
        //        }
        //        else
        //        {
        //            await couponsDocRef.UpdateAsync(new Dictionary<string, object>
        //    {
        //        { couponId, true }
        //    });
        //            Debug.Log($"Coupon {couponId} saved to Firebase successfully.");
        //        }
        //    }
        //    catch (Exception e)
        //    {
        //        Debug.LogError($"Failed to save used coupon {couponId} to Firebase: " + e.Message);
        //    }
        //}

        ///// <summary>
        ///// Saves the progress of a quest to Firebase Firestore.
        ///// </summary>
        ///// <param name="questId">The ID of the quest.</param>
        ///// <param name="progress">The progress value to save.</param>
        ///// <returns>A task representing the asynchronous operation.</returns>
        //public async Task SaveQuestProgressAsync(int questId, int progress)
        //{
        //    if (user == null)
        //    {
        //        Debug.LogError("No user is logged in.");
        //        return;
        //    }
        //    try
        //    {
        //        DocumentReference questsDocRef = firestore
        //            .Collection("Players")
        //            .Document(user.UserId)
        //            .Collection("Progress")
        //            .Document("Quests");

        //        DocumentSnapshot snapshot = await questsDocRef.GetSnapshotAsync();

        //        if (!snapshot.Exists)
        //        {
        //            // If the document does not exist, create it with the initial quest progress
        //            await questsDocRef.SetAsync(new Dictionary<string, object>
        //    {
        //        { questId.ToString(), progress }
        //    });
        //            Debug.Log($"Quest progress for {questId} created and saved successfully in Firebase.");
        //        }
        //        else
        //        {
        //            // If the document exists, update the quest progress
        //            await questsDocRef.UpdateAsync(new Dictionary<string, object>
        //    {
        //        { questId.ToString(), progress }
        //    });
        //            Debug.Log($"Quest progress for {questId} updated successfully in Firebase.");
        //        }
        //    }
        //    catch (Exception e)
        //    {
        //        Debug.LogError($"Failed to save quest progress for {questId}: {e.Message}");
        //    }
        //}

        ///// <summary>
        ///// Saves the completion status of a quest to Firebase Firestore, including the completion timestamp.
        ///// </summary>
        ///// <param name="questId">The ID of the quest.</param>
        ///// <returns>A task representing the asynchronous operation.</returns>
        //public async Task SaveQuestCompletionAsync(int questId)
        //{
        //    if (user == null)
        //    {
        //        Debug.LogError("No user is logged in.");
        //        return;
        //    }

        //    try
        //    {
        //        DocumentReference questsDocRef = firestore
        //            .Collection("Players")
        //            .Document(user.UserId)
        //            .Collection("Progress")
        //            .Document("Quests");

        //        string questKey = "Complete " + questId.ToString(); // Format key as "Complete X"

        //        // Get the current timestamp
        //        Timestamp completionTimestamp = Timestamp.GetCurrentTimestamp();

        //        // Check if the document exists
        //        DocumentSnapshot snapshot = await questsDocRef.GetSnapshotAsync();

        //        if (snapshot.Exists)
        //        {
        //            await questsDocRef.UpdateAsync(new Dictionary<string, object>
        //    {
        //        { questKey, 1 }, // Assuming 1 represents completion
        //        { questKey + "_Timestamp", completionTimestamp } // Save completion timestamp
        //    });
        //        }
        //        else
        //        {
        //            // If the document doesn't exist, create it with the quest completion status and timestamp
        //            await questsDocRef.SetAsync(new Dictionary<string, object>
        //    {
        //        { questKey, 1 }, // Assuming 1 represents completion
        //        { questKey + "_Timestamp", completionTimestamp } // Save completion timestamp
        //    });
        //        }

        //        Debug.Log($"Quest completion for {questId} saved successfully in Firebase with timestamp.");
        //    }
        //    catch (Exception e)
        //    {
        //        Debug.LogError($"Failed to save quest completion for {questId}: {e.Message}");
        //    }
        //}

        ///// <summary>
        ///// Saves the number of monsters killed by the player to their Firestore document.
        ///// The total number of kills is incremented by the amount achieved in the current game session.
        ///// </summary>
        ///// <param name="monstersKilled">The number of monsters killed in the current session.</param>
        //public async Task SaveMonstersKilledAsync(int monstersKilled)
        //{
        //    if (user == null)
        //    {
        //        Debug.LogError("No user is logged in.");
        //        return;
        //    }
        //    DocumentReference playerDocRef = firestore
        //        .Collection("Players")
        //        .Document(user.UserId);

        //    try
        //    {
        //        DocumentSnapshot snapshot = await playerDocRef.GetSnapshotAsync();
        //        double currentScore = 0;
        //        // If the document exists and the "score" field is present, retrieve the current score.
        //        if (snapshot.Exists && snapshot.ContainsField("score"))
        //        {
        //            currentScore = Convert.ToDouble(snapshot.GetValue<string>("score"));
        //        }
        //        else
        //        {
        //            Debug.Log("No score found for the player, starting from 0.");
        //        }
        //        // Add the new kills to the current score
        //        currentScore += monstersKilled;
        //        // Update the Firestore document with the new score
        //        await playerDocRef.UpdateAsync("score", currentScore.ToString());

        //        Debug.Log($"Monsters killed ({monstersKilled}) added to player's score. New score: {currentScore}");
        //    }
        //    catch (Exception e)
        //    {
        //        Debug.LogError($"Failed to save monsters killed to Firestore: {e.Message}");
        //    }
        //}

        ///// <summary>
        ///// Saves the PlayerCharacterFavourite to Firestore.
        ///// </summary>
        ///// <param name="characterId">The ID of the character to save as favourite.</param>
        //public async Task SavePlayerCharacterFavouriteAsync(int characterId)
        //{
        //    if (user != null)
        //    {
        //        try
        //        {
        //            DocumentReference docRef = firestore.Collection("Players").Document(user.UserId);
        //            DocumentSnapshot snapshot = await docRef.GetSnapshotAsync();

        //            if (snapshot.Exists)
        //            {
        //                Dictionary<string, object> playerData = snapshot.ToDictionary();

        //                if (playerData.ContainsKey("PlayerCharacterFavourite"))
        //                {
        //                    // Update the existing field
        //                    await docRef.UpdateAsync("PlayerCharacterFavourite", characterId);
        //                }
        //                else
        //                {
        //                    // Create the field if it doesn't exist
        //                    await docRef.SetAsync(new Dictionary<string, object> { { "PlayerCharacterFavourite", characterId } }, SetOptions.MergeFields("PlayerCharacterFavourite"));
        //                }

        //                Debug.Log("PlayerCharacterFavourite saved successfully.");
        //            }
        //            else
        //            {
        //                Debug.LogWarning("Player document does not exist.");
        //            }
        //        }
        //        catch (Exception e)
        //        {
        //            Debug.LogError("Failed to save PlayerCharacterFavourite: " + e.Message);
        //        }
        //    }
        //    else
        //    {
        //        Debug.LogError("No user is logged in.");
        //    }
        //}

        //public async Task SaveClaimedRewardAsync(string passItemId)
        //{
        //    if (user == null)
        //    {
        //        Debug.LogError("No user is logged in.");
        //        return;
        //    }

        //    try
        //    {
        //        DocumentReference rewardsDocRef = firestore
        //            .Collection("Players")
        //            .Document(user.UserId)
        //            .Collection("Progress")
        //            .Document("BattlePassRewards");

        //        DocumentSnapshot snapshot = await rewardsDocRef.GetSnapshotAsync();

        //        if (snapshot.Exists)
        //        {
        //            await rewardsDocRef.UpdateAsync(new Dictionary<string, object>
        //    {
        //        { passItemId, true }
        //    });
        //        }
        //        else
        //        {
        //            await rewardsDocRef.SetAsync(new Dictionary<string, object>
        //    {
        //        { passItemId, true }
        //    });
        //        }

        //        Debug.Log($"Claimed reward {passItemId} saved to Firebase.");
        //    }
        //    catch (Exception e)
        //    {
        //        Debug.LogError($"Failed to save claimed reward {passItemId} to Firebase: {e.Message}");
        //    }
        //}
        //public async Task SaveBattlePassProgressAsync(int currentXP, int currentLevel, bool isUnlocked)
        //{
        //    if (user == null)
        //    {
        //        Debug.LogError("No user is logged in.");
        //        return;
        //    }

        //    try
        //    {
        //        DocumentReference battlePassDocRef = firestore
        //            .Collection("Players")
        //            .Document(user.UserId)
        //            .Collection("Progress")
        //            .Document("BattlePass");

        //        Dictionary<string, object> battlePassData = new Dictionary<string, object>
        //{
        //    { "CurrentXP", currentXP },
        //    { "CurrentLevel", currentLevel },
        //    { "IsUnlocked", isUnlocked }
        //};

        //        await battlePassDocRef.SetAsync(battlePassData);
        //        Debug.Log("Battle Pass progress saved to Firebase successfully.");
        //    }
        //    catch (Exception e)
        //    {
        //        Debug.LogError("Failed to save Battle Pass progress: " + e.Message);
        //    }
        //}

        ///// <summary>
        ///// Saves basic character info (skin, level, experience, mastery) to Firestore and updates local PlayerSave.
        ///// </summary>
        ///// <param name="characterId">ID of the character.</param>
        ///// <param name="skinIndex">Index of the selected skin. Use -1 if no skin is selected.</param>
        ///// <param name="level">Character level.</param>
        ///// <param name="currentExp">Current experience points.</param>
        ///// <param name="masteryLevel">Current mastery level.</param>
        ///// <param name="masteryExp">Current mastery experience points.</param>
        //public async Task SaveCharacterBasicInfoAsync(int characterId, int skinIndex, int level, int currentExp, int masteryLevel, int masteryExp)
        //{
        //    try
        //    {
        //        DocumentReference charDocRef = firestore
        //            .Collection("Players")
        //            .Document(user.UserId)
        //            .Collection("Characters")
        //            .Document(characterId.ToString());

        //        Dictionary<string, object> data = new Dictionary<string, object>
        //        {
        //            { "CharacterSelectedSkin", skinIndex },
        //            { "CharacterLevel", level },
        //            { "CharacterCurrentExp", currentExp },
        //            { "CharacterMasteryLevel", masteryLevel },
        //            { "CharacterCurrentMasteryExp", masteryExp }
        //        };

        //        // Update Firestore
        //        await charDocRef.SetAsync(data, SetOptions.MergeAll);
        //        Debug.Log($"Character basic info saved for CharacterId {characterId}.");
        //        ;
        //    }
        //    catch (Exception e)
        //    {
        //        Debug.LogError($"Failed to save character basic info for {characterId}: {e.Message}");
        //    }
        //}

        ///// <summary>
        ///// Saves an item to the sub-collection named by 'slotName' under the character document.
        ///// Each item is stored in its own document, named by the unique item identifier.
        ///// Path: Players/{userId}/Characters/{characterId}/{slotName}/{uniqueItemGuid}
        ///// </summary>
        ///// <param name="characterId">ID of the character.</param>
        ///// <param name="slotName">Slot name (e.g. "WeaponSlot" or "RuneSlot").</param>
        ///// <param name="uniqueItemGuid">Unique ID for this item doc.</param>
        ///// <param name="itemId">ID of the item (scriptable or otherwise).</param>
        ///// <param name="itemLevel">Level of the item.</param>
        //public async Task SaveCharacterItemAsync(int characterId, string slotName, string uniqueItemGuid, string itemId, int itemLevel)
        //{
        //    try
        //    {
        //        // Example: /Players/{userId}/Characters/{characterId}/{slotName}/{uniqueItemGuid}
        //        DocumentReference itemDocRef = firestore
        //            .Collection("Players")
        //            .Document(user.UserId)
        //            .Collection("Characters")
        //            .Document(characterId.ToString())
        //            .Collection(slotName)  // <-- The "slotName" subcollection
        //            .Document(uniqueItemGuid);

        //        Dictionary<string, object> data = new Dictionary<string, object>
        //{
        //    { "ItemId", itemId },
        //    { "ItemLevel", itemLevel }
        //};

        //        await itemDocRef.SetAsync(data, SetOptions.MergeAll);
        //        Debug.Log($"[SaveCharacterItemAsync] Item saved: Char {characterId}, Slot {slotName}, Doc {uniqueItemGuid}.");
        //    }
        //    catch (Exception e)
        //    {
        //        Debug.LogError($"Failed to save item for CharId {characterId}, slot {slotName}: {e.Message}");
        //    }
        //}

        ///// <summary>
        ///// Removes (unequips) an item document from the specified slot of the character.
        ///// Path: Players/{userId}/Characters/{characterId}/{slotName}/{uniqueItemGuid}
        ///// </summary>
        //public async Task RemoveCharacterItemAsync(int characterId, string slotName, string uniqueItemGuid)
        //{
        //    try
        //    {
        //        DocumentReference docRef = firestore
        //            .Collection("Players")
        //            .Document(user.UserId)
        //            .Collection("Characters")
        //            .Document(characterId.ToString())
        //            .Collection(slotName)
        //            .Document(uniqueItemGuid);

        //        await docRef.DeleteAsync();
        //        Debug.Log($"[RemoveCharacterItemAsync] Removed item {uniqueItemGuid} from slot {slotName} of Char {characterId}.");
        //    }
        //    catch (Exception e)
        //    {
        //        Debug.LogError($"Failed to remove item {uniqueItemGuid} from char {characterId}, slot {slotName}: {e.Message}");
        //    }
        //}

        ///// <summary>
        ///// Saves a purchased inventory item into Firestore (Players/{userId}/PurchasedItems/Items/{uniqueItemGuid}).
        ///// Also updates local data if desired.
        ///// </summary>
        //public async Task SavePurchasedInventoryItemAsync(PurchasedInventoryItem purchasedItem)
        //{
        //    if (user == null)
        //    {
        //        Debug.LogError("No user is logged in.");
        //        return;
        //    }

        //    try
        //    {
        //        DocumentReference itemDocRef = firestore
        //            .Collection("Players")
        //            .Document(user.UserId)
        //            .Collection("PurchasedItems")
        //            .Document("Items")
        //            .Collection("List")
        //            .Document(purchasedItem.uniqueItemGuid);

        //        Dictionary<string, object> data = new Dictionary<string, object>
        //{
        //    { "uniqueItemGuid", purchasedItem.uniqueItemGuid },
        //    { "itemId", purchasedItem.itemId },
        //    { "itemLevel", purchasedItem.itemLevel }
        //};
        //        Dictionary<string, object> upgradesDict = new Dictionary<string, object>();
        //        if (purchasedItem.upgrades != null)
        //        {
        //            foreach (var kvp in purchasedItem.upgrades)
        //            {
        //                upgradesDict[kvp.Key.ToString()] = kvp.Value;
        //            }
        //        }
        //        data["itemUpgrades"] = upgradesDict;

        //        await itemDocRef.SetAsync(data, SetOptions.MergeAll);
        //        Debug.Log($"Purchased item saved: {purchasedItem.uniqueItemGuid} -> {purchasedItem.itemId}.");
        //    }
        //    catch (Exception e)
        //    {
        //        Debug.LogError($"Failed to save purchased inventory item {purchasedItem.uniqueItemGuid}: {e.Message}");
        //    }
        //}

        ///// <summary>
        ///// Deletes a purchased inventory item from Firestore: 
        /////  Players/{userId}/PurchasedItems/Items/List/{uniqueItemGuid}.
        ///// Also checks all characters to remove the item from sub-collection Items if present.
        ///// Finally, removes it locally from MonetizationManager and InventorySave.
        ///// </summary>
        ///// <param name="uniqueItemGuid">The unique item GUID to delete.</param>
        //public async Task DeletePurchasedInventoryItemAsync(string uniqueItemGuid)
        //{
        //    if (user == null)
        //    {
        //        Debug.LogError("No user is logged in.");
        //        return;
        //    }

        //    try
        //    {
        //        // 1) Remove from "PurchasedItems"
        //        DocumentReference itemDocRef = firestore
        //            .Collection("Players")
        //            .Document(user.UserId)
        //            .Collection("PurchasedItems")
        //            .Document("Items")
        //            .Collection("List")
        //            .Document(uniqueItemGuid);

        //        await itemDocRef.DeleteAsync();
        //        Debug.Log($"Item {uniqueItemGuid} deleted from PurchasedItems.");

        //        // 2) Check all characters for this item
        //        foreach (var cd in GameInstance.Singleton.characterData)
        //        {
        //            int charId = cd.characterId;
        //            // Attempt to remove from the character's "Items" sub-collection
        //            DocumentReference charItemDoc = firestore
        //                .Collection("Players")
        //                .Document(user.UserId)
        //                .Collection("Characters")
        //                .Document(charId.ToString())
        //                .Collection("Items")
        //                .Document(uniqueItemGuid);

        //            await charItemDoc.DeleteAsync();
        //            // It's okay if doc doesn't exist, no error is thrown for a missing doc
        //        }

        //        // 3) Remove locally from MonetizationManager
        //        var allItems = MonetizationManager.Singleton.GetPurchasedInventoryItems();
        //        var foundItem = allItems.Find(i => i.uniqueItemGuid == uniqueItemGuid);
        //        if (foundItem != null)
        //        {
        //            allItems.Remove(foundItem);
        //            // Update the manager
        //            MonetizationManager.Singleton.UpdatePurchasedItems(
        //                MonetizationManager.Singleton.GetPurchasedCharacters(),
        //                MonetizationManager.Singleton.GetPurchasedIcons(),
        //                MonetizationManager.Singleton.GetPurchasedFrames(),
        //                MonetizationManager.Singleton.GetPurchasedShopItems(),
        //                allItems
        //            );
        //        }

        //        Debug.Log($"Item {uniqueItemGuid} fully deleted locally and online.");
        //    }
        //    catch (Exception e)
        //    {
        //        Debug.LogError($"Failed to delete item {uniqueItemGuid}: {e.Message}");
        //    }
        //}

        ///// <summary>
        ///// Saves daily rewards data to Firebase.
        ///// </summary>
        //public async Task SaveDailyRewardsDataAsync(DailyRewardsData dailyData)
        //{
        //    try
        //    {
        //        if (user == null) return;

        //        DocumentReference docRef = firestore
        //            .Collection("Players")
        //            .Document(user.UserId)
        //            .Collection("DailyRewards")
        //            .Document("RewardData");

        //        Dictionary<string, object> saveData = new Dictionary<string, object>
        //        {
        //            { "firstClaimDate", Timestamp.FromDateTime(dailyData.firstClaimDate.ToUniversalTime()) },
        //            { "claimedRewards", dailyData.claimedRewards }
        //        };

        //        await docRef.SetAsync(saveData);
        //    }
        //    catch (Exception e)
        //    {
        //        Debug.LogError($"Failed to save daily rewards data: {e.Message}");
        //    }
        //}

        ///// <summary>
        ///// Saves new player rewards data to Firebase.
        ///// </summary>
        //public async Task SaveNewPlayerRewardsDataAsync(NewPlayerRewardsData newPlayerData)
        //{
        //    try
        //    {
        //        if (user == null) return;

        //        DocumentReference docRef = firestore
        //            .Collection("Players")
        //            .Document(user.UserId)
        //            .Collection("NewPlayerRewards")
        //            .Document("RewardData");

        //        Dictionary<string, object> saveData = new Dictionary<string, object>
        //        {
        //            { "accountCreationDate", Timestamp.FromDateTime(newPlayerData.accountCreationDate.ToUniversalTime()) },
        //            { "claimedRewards", newPlayerData.claimedRewards }
        //        };

        //        await docRef.SetAsync(saveData);
        //    }
        //    catch (Exception e)
        //    {
        //        Debug.LogError($"Failed to save new player rewards data: {e.Message}");
        //    }
        //}

    }
}
