//using Firebase;
//using Firebase.Auth;
//using Firebase.Firestore;
using System;
using System.Threading.Tasks;
using UnityEngine;

namespace BulletHellTemplate
{
    /// <summary>
    /// Handles Firebase-specific authentication, storing the authenticated user,
    /// and delegating data loading to FirebaseManager.
    /// </summary>
    public class FirebaseAuthManager : MonoBehaviour
    {
    //    [Tooltip("Singleton instance for global access.")]
    //    public static FirebaseAuthManager Singleton;

    //    private bool instanceInitialized;
    //    private FirebaseAuth firebaseAuth;
    //    private FirebaseFirestore firebaseFirestore;
    //    private FirebaseUser currentUser;

    //    private void Awake()
    //    {
    //        if (Singleton == null)
    //        {
    //            Singleton = this;
    //            DontDestroyOnLoad(gameObject);
    //        }
    //        else
    //        {
    //            Destroy(gameObject);
    //        }
    //    }

    //    /// <summary>
    //    /// Indicates if this FirebaseAuthManager was successfully initialized.
    //    /// </summary>
    //    /// <returns>True if initialized, false otherwise.</returns>
    //    public bool IsInitialized()
    //    {
    //        return instanceInitialized;
    //    }

    //    /// <summary>
    //    /// Initializes FirebaseAuthManager with the provided FirebaseAuth and FirebaseFirestore instances.
    //    /// </summary>
    //    /// <param name="auth">FirebaseAuth instance.</param>
    //    /// <param name="firestore">FirebaseFirestore instance.</param>
    //    public void InitializeAuthBackend(FirebaseAuth auth, FirebaseFirestore firestore)
    //    {
    //        firebaseAuth = auth;
    //        firebaseFirestore = firestore;
    //        currentUser = firebaseAuth.CurrentUser;
    //        instanceInitialized = true;
    //    }

    //    /// <summary>
    //    /// Returns the current FirebaseUser, or null if none is logged in.
    //    /// </summary>
    //    public FirebaseUser GetCurrentUser()
    //    {
    //        return currentUser;
    //    }

    //    /// <summary>
    //    /// Logs in a user with email and password using FirebaseAuth.
    //    /// </summary>
    //    /// <param name="email">The user's email.</param>
    //    /// <param name="password">The user's password.</param>
    //    public async Task LoginWithEmailAsync(string email, string password)
    //    {
    //        if (!instanceInitialized)
    //        {
    //            throw new Exception("FirebaseAuthManager not initialized.");
    //        }
    //        await firebaseAuth.SignInWithEmailAndPasswordAsync(email, password);
    //        currentUser = firebaseAuth.CurrentUser;
    //    }

    //    /// <summary>
    //    /// Logs in a user anonymously using FirebaseAuth.
    //    /// </summary>
    //    public async Task LoginAnonymouslyAsync()
    //    {
    //        if (!instanceInitialized)
    //        {
    //            throw new Exception("FirebaseAuthManager not initialized.");
    //        }
    //        await firebaseAuth.SignInAnonymouslyAsync();
    //        currentUser = firebaseAuth.CurrentUser;
    //    }

    //    /// <summary>
    //    /// Creates a new user account with the specified email and password.
    //    /// </summary>
    //    /// <param name="email">User's email.</param>
    //    /// <param name="password">User's password.</param>
    //    public async Task CreateAccountAsync(string email, string password)
    //    {
    //        if (!instanceInitialized)
    //        {
    //            throw new Exception("FirebaseAuthManager not initialized.");
    //        }
    //        await firebaseAuth.CreateUserWithEmailAndPasswordAsync(email, password);
    //        currentUser = firebaseAuth.CurrentUser;
    //    }

    //    /// <summary>
    //    /// Calls FirebaseManager to load and synchronize the current user's data from Firestore.
    //    /// Returns a message describing the load result.
    //    /// </summary>
    //    public async Task<string> LoadPlayerDataAsync()
    //    {
    //        if (!instanceInitialized)
    //        {
    //            return "FirebaseAuthManager is not initialized.";
    //        }
    //        if (currentUser == null)
    //        {
    //            return "No user is currently logged in.";
    //        }

    //        string userId = currentUser.UserId;
    //        return await FirebaseManager.Singleton.LoadAndSyncPlayerData(userId);
    //    }
    }
}
