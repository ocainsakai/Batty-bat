using System;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using UnityEngine;
using Newtonsoft.Json;
using System.Collections.Generic;
using System.IO;

namespace BulletHellTemplate
{
    /// <summary>
    /// SecurePrefs
    /// ===========
    /// A thin cryptographic wrapper around Unity `PlayerPrefs`.
    ///
    /// • **Confidentiality** – AES-128-CBC with PKCS#7 padding.  
    /// • **Integrity**       – HMAC-SHA-256 over IV‖CipherText.  
    /// • **Format**          – Base-64( IV ‖ CIPHERTEXT ‖ HMAC ).
    /// </summary>
    public static class SecurePrefs
    {
        private const string MasterPassphrase = "bh-template-very-secret-passphrase";
        private static readonly byte[] _masterKey =
            SHA256.Create().ComputeHash(Encoding.UTF8.GetBytes(MasterPassphrase));

        private static readonly byte[] _encKey = _masterKey.Take(16).ToArray();   // AES-128
        private static readonly byte[] _macKey = _masterKey.Skip(16).Take(16).ToArray();
        private const int AesBlock = 16;                                          // IV length

        /*────────────────────────── High-level API ──────────────────────────*/

        #region String / Int / Float helpers

        public static void SetEncryptedString(string key, string value, bool autoSave = true)
        {
            byte[] blob = Encrypt(Encoding.UTF8.GetBytes(value));
            PlayerPrefs.SetString(key, Convert.ToBase64String(blob));
            if (autoSave) PlayerPrefs.Save();
        }

        public static string GetDecryptedString(string key, string defaultValue = "")
        {
            if (!PlayerPrefs.HasKey(key)) return defaultValue;

            try
            {
                byte[] blob = Convert.FromBase64String(PlayerPrefs.GetString(key));
                byte[] plain = Decrypt(blob);
                return Encoding.UTF8.GetString(plain);
            }
            catch
            {
                return defaultValue;
            }
        }


        public static void SetEncryptedInt(string key, int value, bool autoSave = true)
            => SetEncryptedString(key, value.ToString(), autoSave);

        public static int GetDecryptedInt(string key, int defaultValue = 0)
            => int.TryParse(GetDecryptedString(key, defaultValue.ToString()), out var v) ? v : defaultValue;

        public static void SetEncryptedFloat(string key, float value, bool autoSave = true)
            => SetEncryptedString(key, value.ToString("R"), autoSave);

        public static float GetDecryptedFloat(string key, float defaultValue = 0f) =>
            float.TryParse(GetDecryptedString(key, defaultValue.ToString("R")), out var v) ? v : defaultValue;

        public static void SetEncryptedObject<T>(string key, T obj, bool autoSave = true)
        {
            string json = JsonConvert.SerializeObject(obj);
            SetEncryptedString(key, json, autoSave);
        }

        public static T GetDecryptedObject<T>(string key, T defaultValue = default)
        {
            string json = GetDecryptedString(key);
            return string.IsNullOrEmpty(json) ? defaultValue : JsonConvert.DeserializeObject<T>(json);
        }

        #endregion

        #region Convenience (HasKey / DeleteKey)
        public static bool HasKey(string key) => PlayerPrefs.HasKey(key);

        public static void DeleteKey(string key, bool autoSave = true)
        {
            if (PlayerPrefs.HasKey(key))
            {
                PlayerPrefs.DeleteKey(key);
                if (autoSave) PlayerPrefs.Save();
            }
        }
        #endregion

        // ─────────── File Storage Settings ───────────
        private const string FileName = "secure_prefs.json";
        private static readonly string FilePath =
            Path.Combine(Application.persistentDataPath, FileName);
        private static Dictionary<string, string> _filePrefs;

        #region File-based API

        /// <summary>
        /// Loads the file-based preferences into memory (lazy).
        /// </summary>
        private static void LoadFilePrefs()
        {
            if (_filePrefs != null) return;
            try
            {
                if (File.Exists(FilePath))
                {
                    string text = File.ReadAllText(FilePath);
                    _filePrefs = JsonConvert.DeserializeObject<Dictionary<string, string>>(text)
                                 ?? new Dictionary<string, string>();
                }
                else
                {
                    _filePrefs = new Dictionary<string, string>();
                }
            }
            catch
            {
                _filePrefs = new Dictionary<string, string>();
            }
        }

        /// <summary>
        /// Persists the in-memory dictionary to disk as JSON.
        /// </summary>
        private static void SaveFilePrefs()
        {
            try
            {
                string json = JsonConvert.SerializeObject(_filePrefs, Formatting.Indented);
                File.WriteAllText(FilePath, json);
            }
            catch (Exception e)
            {
                Debug.LogError($"SecurePrefs: failed to save file prefs: {e}");
            }
        }

        /// <summary>
        /// Encrypts and stores a string under the given key in the JSON file.
        /// </summary>
        public static void SetEncryptedStringToFile(string key, string value)
        {
            byte[] blob = Encrypt(Encoding.UTF8.GetBytes(value));
            string encoded = Convert.ToBase64String(blob);
            LoadFilePrefs();
            _filePrefs[key] = encoded;
            SaveFilePrefs();
        }
        /// <summary>
        /// Encrypts and stores an integer under the given key in the JSON file.
        /// </summary>
        /// <param name="key">Preference key.</param>
        /// <param name="value">Integer value to encrypt and store.</param>
        public static void SetEncryptedIntToFile(string key, int value)
        {
            SetEncryptedStringToFile(key, value.ToString());
        }

        /// <summary>
        /// Retrieves and decrypts an integer from the JSON file store.
        /// </summary>
        /// <param name="key">Preference key.</param>
        /// <param name="defaultValue">Value to return if key is missing or invalid.</param>
        /// <returns>Decrypted integer or defaultValue.</returns>
        public static int GetDecryptedIntFromFile(string key, int defaultValue = 0)
        {
            string str = GetDecryptedStringFromFile(key, defaultValue.ToString());
            return int.TryParse(str, out var v) ? v : defaultValue;
        }

        /// <summary>
        /// Encrypts and stores a float under the given key in the JSON file.
        /// </summary>
        /// <param name="key">Preference key.</param>
        /// <param name="value">Float value to encrypt and store.</param>
        public static void SetEncryptedFloatToFile(string key, float value)
        {
            SetEncryptedStringToFile(key, value.ToString("R"));
        }

        /// <summary>
        /// Retrieves and decrypts a float from the JSON file store.
        /// </summary>
        /// <param name="key">Preference key.</param>
        /// <param name="defaultValue">Value to return if key is missing or invalid.</param>
        /// <returns>Decrypted float or defaultValue.</returns>
        public static float GetDecryptedFloatFromFile(string key, float defaultValue = 0f)
        {
            string str = GetDecryptedStringFromFile(key, defaultValue.ToString("R"));
            return float.TryParse(str, out var v) ? v : defaultValue;
        }

        /// <summary>
        /// Serializes, encrypts and stores an object of type T under the given key in the JSON file.
        /// </summary>
        /// <typeparam name="T">Type of object to store.</typeparam>
        /// <param name="key">Preference key.</param>
        /// <param name="obj">Object to serialize and encrypt.</param>
        public static void SetEncryptedObjectToFile<T>(string key, T obj)
        {
            string json = JsonConvert.SerializeObject(obj);
            SetEncryptedStringToFile(key, json);
        }

        /// <summary>
        /// Retrieves, decrypts and deserializes an object of type T from the JSON file store.
        /// </summary>
        /// <typeparam name="T">Type of object to retrieve.</typeparam>
        /// <param name="key">Preference key.</param>
        /// <param name="defaultValue">Value to return if key is missing or invalid.</param>
        /// <returns>Deserialized object or defaultValue.</returns>
        public static T GetDecryptedObjectFromFile<T>(string key, T defaultValue = default)
        {
            string json = GetDecryptedStringFromFile(key, null);
            if (string.IsNullOrEmpty(json))
                return defaultValue;
            try
            {
                return JsonConvert.DeserializeObject<T>(json);
            }
            catch
            {
                return defaultValue;
            }
        }

        /// <summary>
        /// Retrieves and decrypts a string from the JSON file store.
        /// </summary>
        public static string GetDecryptedStringFromFile(string key, string defaultValue = "")
        {
            LoadFilePrefs();
            if (!_filePrefs.TryGetValue(key, out var encoded))
                return defaultValue;

            try
            {
                byte[] blob = Convert.FromBase64String(encoded);
                byte[] plain = Decrypt(blob);
                return Encoding.UTF8.GetString(plain);
            }
            catch
            {
                return defaultValue;
            }
        }

        /// <summary>
        /// Checks if the given key exists in the JSON file.
        /// </summary>
        public static bool HasKeyInFile(string key)
        {
            LoadFilePrefs();
            return _filePrefs.ContainsKey(key);
        }

        /// <summary>
        /// Deletes the specified key from the JSON file and saves.
        /// </summary>
        public static void DeleteKeyFromFile(string key)
        {
            LoadFilePrefs();
            if (_filePrefs.Remove(key))
                SaveFilePrefs();
        }

        /// <summary>
        /// Clears all entries in the JSON file for a completely fresh login.
        /// </summary>
        public static void ClearFilePrefs()
        {
            _filePrefs = new Dictionary<string, string>();
            SaveFilePrefs();
        }

        #endregion

        /*────────────────────────── Crypto internals ────────────────────────*/

        private static byte[] Encrypt(byte[] plain)
        {
            using var aes = Aes.Create();
            aes.Key = _encKey;
            aes.Mode = CipherMode.CBC;
            aes.Padding = PaddingMode.PKCS7;
            aes.GenerateIV();

            byte[] cipher = aes.CreateEncryptor().TransformFinalBlock(plain, 0, plain.Length);

            // blob = IV ‖ CIPHERTEXT ‖ HMAC
            var blob = new byte[aes.IV.Length + cipher.Length + 32];
            Buffer.BlockCopy(aes.IV, 0, blob, 0, aes.IV.Length);
            Buffer.BlockCopy(cipher, 0, blob, aes.IV.Length, cipher.Length);

            byte[] tag = Hmac(blob, 0, aes.IV.Length + cipher.Length);
            Buffer.BlockCopy(tag, 0, blob, aes.IV.Length + cipher.Length, tag.Length);

            return blob;
        }

        private static byte[] Decrypt(byte[] blob)
        {
            if (blob.Length < AesBlock + 32)
                throw new CryptographicException("Encrypted blob too small.");

            int ivLen = AesBlock;
            int tagLen = 32;
            int cipherLen = blob.Length - ivLen - tagLen;

            // verify HMAC
            if (!Hmac(blob, 0, ivLen + cipherLen).SequenceEqual(blob.Skip(ivLen + cipherLen)))
                throw new CryptographicException("HMAC validation failed – data tampered.");

            byte[] iv = blob.Take(ivLen).ToArray();
            byte[] cipher = blob.Skip(ivLen).Take(cipherLen).ToArray();

            using var aes = Aes.Create();
            aes.Key = _encKey;
            aes.Mode = CipherMode.CBC;
            aes.Padding = PaddingMode.PKCS7;
            aes.IV = iv;

            return aes.CreateDecryptor().TransformFinalBlock(cipher, 0, cipher.Length);
        }

        private static byte[] Hmac(byte[] buf, int off, int cnt)
        {
            using var h = new HMACSHA256(_macKey);
            return h.ComputeHash(buf, off, cnt);
        }

        // ─────────── Keys to clear for a fresh login ───────────

        /// <summary>
        /// Clears the specified SecurePrefs/PlayerPrefs keys for a completely clean login state.
        /// </summary>
        public static void ClearSecurePrefsData()
        {
            PlayerSave.ClearAllPurchased();           
            PlayerPrefs.DeleteAll();
            PlayerPrefs.Save();
        }

    }
}
