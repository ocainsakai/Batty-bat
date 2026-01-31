using Colyseus;
using Cysharp.Threading.Tasks;
using GameDevWare.Serialization;
using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using UnityEngine;

namespace BulletHellTemplate
{
    /// <summary>
    /// Handles Colyseus authentication (guest and email) and
    /// keeps credentials locally to allow seamless auto-login.
    /// The only way to lose the account is by calling <see cref="LogOut"/>.
    /// </summary>
    public sealed class WebsocketAuthHandler
    {
        public string Token { get; private set; }
        public int UserId { get; private set; }


        /* ────── PlayerPrefs keys ────── */
        private const string PREF_TOKEN = "colyseus_jwt";
        private const string PREF_UID = "colyseus_uid";
        private const string PREF_EMAIL = "colyseus_email";
        private const string PREF_PASS = "colyseus_pass";

        private readonly ColyseusClient _client;
        private static readonly Regex EmailRx =
            new(@"^[^@\s]+@[^@\s]+\.[^@\s]+$", RegexOptions.Compiled);

        public WebsocketAuthHandler(string url) => _client = new ColyseusClient(url);
        public HTTP Http => _client.Http;

        /*────────────────── Public API ──────────────────*/

        /// <summary>
        /// Attempts to restore the last session.<br/>
        /// Strategy: <br/>
        /// 1. Validate saved JWT (<c>/auth/me</c>)<br/>
        /// 2. If expired ⇒ login again using stored e-mail + password<br/>
        /// 3. If that also fails ⇒ creates a fresh guest<br/>
        /// </summary>
        public async UniTask<bool> TryAutoLoginAsync()
        {
            /* Step 0: nothing saved ⇒ fail fast */
            if (!SecurePrefs.HasKeyInFile(PREF_TOKEN))
                return false;

            Token = SecurePrefs.GetDecryptedStringFromFile(PREF_TOKEN);
            _client.Http.AuthToken = Token;
            UserId = SecurePrefs.GetDecryptedIntFromFile(PREF_UID, 0);

            /* Step 1: is the token still valid? */
            if (await ValidateTokenAsync()) return true;

            /* Step 2: try e-mail / pass fallback */
            if (SecurePrefs.HasKeyInFile(PREF_EMAIL) &&
                SecurePrefs.HasKeyInFile(PREF_PASS))
            {
                string email = SecurePrefs.GetDecryptedStringFromFile(PREF_EMAIL);
                string pass = SecurePrefs.GetDecryptedStringFromFile(PREF_PASS);

                var res = await LoginAsync(email, pass);
                if (res.Success) return true; // success → new token saved
            }

            /* Step 3: create a brand-new guest */
            var guest = await LoginGuestAsync();
            return guest.Success;
        }

        public UniTask<RequestResult> RegisterAsync(string email, string pass) =>
            EmailRx.IsMatch(email)
                ? RequestAuth("auth/register", email, pass)
                : UniTask.FromResult(RequestResult.Fail("INVALID_EMAIL"));

        public UniTask<RequestResult> LoginAsync(string email, string pass) =>
            EmailRx.IsMatch(email)
                ? RequestAuth("auth/login", email, pass)
                : UniTask.FromResult(RequestResult.Fail("INVALID_EMAIL"));

        public async UniTask<RequestResult> LoginGuestAsync()
        {
            string raw = await _client.Http.Request("POST", "auth/anonymous", null);
            return ParseAndPersist(raw, isGuest: true);
        }

        public void LogOut()
        {
            SecurePrefs.DeleteKeyFromFile(PREF_TOKEN);
            SecurePrefs.DeleteKeyFromFile(PREF_UID);
            SecurePrefs.DeleteKeyFromFile(PREF_EMAIL);
            SecurePrefs.DeleteKeyFromFile(PREF_PASS);

            Token = null;
            UserId = 0;
            _client.Http.AuthToken = null;
        }

        /*────────────────── Internals ──────────────────*/

        private async UniTask<bool> ValidateTokenAsync()
        {
            try
            {
                await _client.Http.Request("GET", "auth/me");
                return true;
            }
            catch { return false; }
        }

        private async UniTask<RequestResult> RequestAuth(string path, string email, string pass)
        {
            try
            {
                string raw = await _client.Http.Request("POST", path,
                    new Dictionary<string, object> { { "email", email }, { "password", pass } });

                return ParseAndPersist(raw, isGuest: false, email, pass);
            }
            catch (HttpException e)
            {
                Debug.LogError($"[Auth] HTTP error: {e.Message}");
                if (e.Message.Contains("invalid_credentials", StringComparison.OrdinalIgnoreCase))
                {
                    BackendManager.Service?.Logout(); 
                }

                return RequestResult.Fail("invalid_credentials");
            }
        }

        private RequestResult ParseAndPersist(string raw, bool isGuest,
                                       string email = null, string pass = null)
        {
            if (!raw.TrimStart().StartsWith("{"))
                return RequestResult.Fail(raw.Trim('"'));

            var obj = Json.Deserialize<IndexedDictionary<string, object>>(raw);

            var user = (obj.TryGetValue("user", out var u) &&
                u is IndexedDictionary<string, object> ud) ? ud : obj;

            /* ---------- JWT & UID ---------- */
            Token = obj["token"] as string;
            UserId = (user.TryGetValue("id", out var v) &&
                      int.TryParse(v.ToString(), out var id)) ? id : 0;

            _client.Http.AuthToken = Token;
            SecurePrefs.SetEncryptedStringToFile(PREF_TOKEN, Token);
            SecurePrefs.SetEncryptedIntToFile(PREF_UID, UserId);
            /* ---------- Credentials ---------- */
            // Guest: email + guestPass
            if (isGuest)                      
            {
                email = user.TryGetValue("email", out var e) ? e as string : null;
                pass = user.TryGetValue("guestPass", out var p) ? p as string : null;
            }
            if (!string.IsNullOrEmpty(email) && !string.IsNullOrEmpty(pass))
            {
                SecurePrefs.SetEncryptedStringToFile(PREF_EMAIL, email);
                SecurePrefs.SetEncryptedStringToFile(PREF_PASS, pass);
            }
            return RequestResult.Ok();
        }
    }
}
