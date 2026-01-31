using System.Collections.Generic;
using UnityEngine;
using System.IO;
using System.Xml.Serialization;
using System;
using System.Linq;
using TMPro;
using System.Reflection;

namespace LanguageManager
{
    public enum LanguageMode
    {
        InternalDatabase,
        ExternalFiles,
        ComponentBased,
        GlobalExternalFiles // Additional mode, if necessary
    }

    [System.Serializable]
    public class Language
    {
        [Tooltip("The unique identifier for the language.")]
        public string LanguageID;

        [Tooltip("The display title for the language.")]
        public string LanguageTitle;

        [Tooltip("The icon representing the language.")]
        public Sprite LanguageIcon;

        [Tooltip("If true, the font asset will be swapped when changing the language.")]
        public bool changeFontOnSwap;

        [Tooltip("The TextMeshPro font asset to be used when the language is active.")]
        public TMP_FontAsset fontAssetTextMesh;

        [Tooltip("The Unity UI font asset to be used when the language is active.")]
        public Font fontAsset;

        [Tooltip("If true, the text will be displayed in bold.")]
        public bool applyBold;

        [Tooltip("If true, all text will be converted to uppercase.")]
        public bool applyUppercase;
    }

    [System.Serializable]
    public class Translation
    {
        public string LanguageID;
        public string TranslatedText;
    }

    [System.Serializable]
    public class TextEntry
    {
        public string TextID;
        public List<Translation> Translations = new List<Translation>();
    }

   
    public class LanguageManager : MonoBehaviour
    {
        public static LanguageManager Instance;
        public bool disableTMPWarnings;
        public bool enableTextFormatting;
        public LanguageMode languageMode;
        public string currentLanguageID;

        public List<Language> Languages = new List<Language>();
        public List<TextEntry> TextEntries = new List<TextEntry>();

        public delegate void OnLanguageChanged();
        public static event OnLanguageChanged onLanguageChanged;

        private Dictionary<string, string> externalTranslations = new Dictionary<string, string>();

        private void Awake()
        {
            if (disableTMPWarnings)
            {
                DisableTMPWarnings();
            }
            if (Instance == null)
            {
                Instance = this;
                DontDestroyOnLoad(gameObject);
            }
            else
            {
                Destroy(gameObject);
            }

            string savedLanguage = PlayerPrefs.GetString("SelectedLanguage", currentLanguageID);
            SetLanguage(savedLanguage);
        }

        private void Start()
        {
            if (languageMode == LanguageMode.InternalDatabase)
            {
                Debug.Log("InternalDatabase mode selected.");
                // Data is already available in the internal lists
            }
            else if (languageMode == LanguageMode.ExternalFiles)
            {
                Debug.Log("ExternalFiles mode selected.");
                LoadExternalLanguageFiles();
            }
            else if (languageMode == LanguageMode.GlobalExternalFiles)
            {
                Debug.Log("GlobalExternalFiles mode selected.");
                LoadGlobalLanguageFile();
            }
        }

        /// <summary>
        /// Disables TextMeshPro warnings by setting the private m_warningsDisabled field to true.
        /// </summary>
        private void DisableTMPWarnings()
        {
            TMP_Settings settings = TMP_Settings.instance;
            if (settings != null)
            {
                FieldInfo field = typeof(TMP_Settings).GetField("m_warningsDisabled", BindingFlags.NonPublic | BindingFlags.Instance);
                if (field != null)
                {
                    field.SetValue(settings, true);
                    Debug.Log("TMP warnings disabled successfully.");
                }
                else
                {
                    Debug.LogWarning("Unable to find the field m_warningsDisabled in TMP_Settings.");
                }
            }
            else
            {
                Debug.LogWarning("TMP_Settings instance not found. Make sure TMP Settings asset exists in Resources.");
            }
        }


        /// <summary>
        /// Sets the current language by its ID, updates the translations, and saves the selection locally.
        /// </summary>
        /// <param name="languageID">The ID of the language to set.</param>
        public void SetLanguage(string languageID)
        {
            if (string.IsNullOrEmpty(languageID))
            {
                Debug.LogError("SetLanguage called with null or empty languageID.");
                return;
            }

            currentLanguageID = languageID;

            if (languageMode == LanguageMode.ExternalFiles)
            {
                LoadExternalLanguageFiles();
            }
            else if (languageMode == LanguageMode.GlobalExternalFiles)
            {
                LoadGlobalLanguageFile();
            }

            // Save the selected language locally
            PlayerPrefs.SetString("SelectedLanguage", languageID);
            PlayerPrefs.Save();
            // Notify all components that the language has changed
            onLanguageChanged?.Invoke();
        }

        /// <summary>
        /// Loads translations from individual language files in StreamingAssets. Supports JSON, XML, and CSV formats.
        /// </summary>
        private void LoadExternalLanguageFiles()
        {
            externalTranslations.Clear();

            // Define the path to the Languages folder
            string languagesFolderPath = Path.Combine(Application.streamingAssetsPath, "Languages");

            // Ensure the folder exists
            if (!Directory.Exists(languagesFolderPath))
            {
                Debug.LogError($"Languages folder not found at {languagesFolderPath}");
                return;
            }

            try
            {
                // Load the available languages based on the file names
                PopulateAvailableLanguagesFromFiles(languagesFolderPath);

                // Define possible file paths for the current language file
                string jsonFilePath = Path.Combine(languagesFolderPath, currentLanguageID + ".json");
                string xmlFilePath = Path.Combine(languagesFolderPath, currentLanguageID + ".xml");
                string csvFilePath = Path.Combine(languagesFolderPath, currentLanguageID + ".csv");

                // Check which format exists and load the language file
                if (File.Exists(jsonFilePath))
                {
                    LoadExternalLanguageFromJSON(jsonFilePath);
                }
                else if (File.Exists(xmlFilePath))
                {
                    LoadExternalLanguageFromXML(xmlFilePath);
                }
                else if (File.Exists(csvFilePath))
                {
                    LoadExternalLanguageFromCSV(csvFilePath);
                }
                else
                {
                    Debug.LogError($"No language file found for {currentLanguageID} in JSON, XML, or CSV format.");
                }
            }
            catch (Exception e)
            {
                Debug.LogError($"Error loading external language file: {e.Message}");
            }
        }

        private void PopulateAvailableLanguagesFromFiles(string folderPath)
        {
            Languages.Clear();

            // Get all language files in the folder (supports .json, .xml, and .csv)
            string[] languageFiles = Directory.GetFiles(folderPath, "*.*")
                                              .Where(file => (file.EndsWith(".json") || file.EndsWith(".xml") || file.EndsWith(".csv"))
                                                             && !Path.GetFileName(file).ToLower().Contains("global")) // Filter out "global" files
                                              .ToArray();

            foreach (string filePath in languageFiles)
            {
                // Extract the language ID from the file name (e.g., "es.json" -> "es")
                string fileName = Path.GetFileNameWithoutExtension(filePath);

                // Create a new language entry with the file name as the LanguageID and LanguageTitle
                Language newLanguage = new Language
                {
                    LanguageID = fileName,
                    LanguageTitle = fileName // You can set this to something more user-friendly if needed
                };

                // Add the language to the list
                Languages.Add(newLanguage);
            }

        }


        /// <summary>
        /// Loads external language file from a JSON file.
        /// </summary>
        private void LoadExternalLanguageFromJSON(string filePath)
        {
            try
            {
                // Read the JSON content from the file
                string jsonContent = File.ReadAllText(filePath);

                // Deserializing the new structure which contains "Translations" directly
                var loadedTranslations = JsonUtility.FromJson<TranslationListWrapper>(jsonContent);

                if (loadedTranslations != null && loadedTranslations.Translations != null)
                {
                    // Populate externalTranslations dictionary
                    foreach (var translation in loadedTranslations.Translations)
                    {
                        externalTranslations[translation.TextID] = translation.TranslatedText;
                    }
                    Debug.Log("Language file loaded successfully from JSON.");
                }
                else
                {
                    Debug.LogError("Failed to deserialize the JSON file or it is empty.");
                }
            }
            catch (Exception e)
            {
                Debug.LogError($"Error loading JSON language file: {e.Message}");
            }
        }

        // Wrapper class for the new JSON format
        [System.Serializable]
        public class TranslationListWrapper
        {
            public List<TranslationSimple> Translations;
        }

        // Simpler structure for individual translations in the new JSON format
        [System.Serializable]
        public class TranslationSimple
        {
            public string TextID;
            public string TranslatedText;
        }


        /// <summary>
        /// Loads external language file from an XML file.
        /// </summary>
        private void LoadExternalLanguageFromXML(string filePath)
        {
            try
            {
                string xmlContent = File.ReadAllText(filePath);
                LanguageDataWrapper loadedData = null;

                XmlSerializer serializer = new XmlSerializer(typeof(LanguageDataWrapper));
                using (StringReader reader = new StringReader(xmlContent))
                {
                    loadedData = (LanguageDataWrapper)serializer.Deserialize(reader);
                }

                if (loadedData != null)
                {
                    foreach (TextEntry entry in loadedData.TextEntries)
                    {
                        Translation translation = entry.Translations.Find(t => t.LanguageID.Equals(currentLanguageID, StringComparison.OrdinalIgnoreCase));
                        if (translation != null)
                        {
                            externalTranslations[entry.TextID] = translation.TranslatedText;
                        }
                    }
                }
                else
                {
                    Debug.LogError("Failed to deserialize the XML file.");
                }
            }
            catch (Exception e)
            {
                Debug.LogError($"Error loading XML language file: {e.Message}");
            }
        }

        /// <summary>
        /// Loads external language file from a CSV file.
        /// </summary>
        private void LoadExternalLanguageFromCSV(string filePath)
        {
            try
            {
                using (StreamReader reader = new StreamReader(filePath))
                {
                    string headerLine = reader.ReadLine();
                    string[] headers = ParseCSVLine(headerLine);

                    while (!reader.EndOfStream)
                    {
                        string line = reader.ReadLine();
                        string[] fields = ParseCSVLine(line);

                        if (fields.Length > 0)
                        {
                            string textID = fields[0];
                            for (int i = 1; i < fields.Length; i++)
                            {
                                if (headers[i].Equals(currentLanguageID, StringComparison.OrdinalIgnoreCase))
                                {
                                    externalTranslations[textID] = fields[i];
                                    break;
                                }
                            }
                        }
                    }
                }
            }
            catch (Exception e)
            {
                Debug.LogError($"Error loading CSV language file: {e.Message}");
            }
        }


        /// <summary>
        /// Loads all translations from a global language file stored in StreamingAssets. Supports XML, JSON, and CSV formats.
        /// </summary>
        private void LoadGlobalLanguageFile()
        {
            externalTranslations.Clear();

            // Definindo os possíveis caminhos dos arquivos em diferentes formatos
            string xmlFilePath = Path.Combine(Application.streamingAssetsPath, "Languages", "globalLanguages.xml");
            string jsonFilePath = Path.Combine(Application.streamingAssetsPath, "Languages", "globalLanguages.json");
            string csvFilePath = Path.Combine(Application.streamingAssetsPath, "Languages", "globalLanguages.csv");

            try
            {
                if (File.Exists(xmlFilePath))
                {
                    LoadGlobalLanguageFromXML(xmlFilePath);
                }
                else if (File.Exists(jsonFilePath))
                {
                    LoadGlobalLanguageFromJSON(jsonFilePath);
                }
                else if (File.Exists(csvFilePath))
                {
                    LoadGlobalLanguageFromCSV(csvFilePath);
                }
                else
                {
                    Debug.LogError("No global language file found in XML, JSON, or CSV format.");
                }
            }
            catch (Exception e)
            {
                Debug.LogError($"Error loading global language file: {e.Message}");
            }
        }

        /// <summary>
        /// Loads the global language file from an XML file.
        /// </summary>
        private void LoadGlobalLanguageFromXML(string filePath)
        {
            try
            {
                string xmlContent = File.ReadAllText(filePath);
                LanguageDataWrapper dataWrapper = null;

                XmlSerializer serializer = new XmlSerializer(typeof(LanguageDataWrapper));
                using (StringReader reader = new StringReader(xmlContent))
                {
                    dataWrapper = (LanguageDataWrapper)serializer.Deserialize(reader);
                }

                if (dataWrapper != null)
                {
                    this.Languages = dataWrapper.Languages;
                    this.TextEntries = dataWrapper.TextEntries;
                    PopulateExternalTranslations();
                }
                else
                {
                    Debug.LogError("Failed to deserialize globalLanguages.xml or it is empty.");
                }
            }
            catch (Exception e)
            {
                Debug.LogError($"Error loading XML language file: {e.Message}");
            }
        }

        /// <summary>
        /// Loads the global language file from a JSON file.
        /// </summary>
        private void LoadGlobalLanguageFromJSON(string filePath)
        {
            try
            {
                string jsonContent = File.ReadAllText(filePath);
                LanguageDataWrapper dataWrapper = JsonUtility.FromJson<LanguageDataWrapper>(jsonContent);

                if (dataWrapper != null)
                {
                    this.Languages = dataWrapper.Languages;
                    this.TextEntries = dataWrapper.TextEntries;
                    PopulateExternalTranslations();
                }
                else
                {
                    Debug.LogError("Failed to deserialize globalLanguages.json or it is empty.");
                }
            }
            catch (Exception e)
            {
                Debug.LogError($"Error loading JSON language file: {e.Message}");
            }
        }

        /// <summary>
        /// Loads the global language file from a CSV file.
        /// </summary>
        private void LoadGlobalLanguageFromCSV(string filePath)
        {
            try
            {
                using (StreamReader reader = new StreamReader(filePath))
                {
                    string headerLine = reader.ReadLine();
                    string[] headers = ParseCSVLine(headerLine);

                    Languages.Clear();
                    TextEntries.Clear();

                    // Populate languages from the header
                    for (int i = 1; i < headers.Length; i++)
                    {
                        Language language = new Language { LanguageID = headers[i], LanguageTitle = headers[i] };
                        Languages.Add(language);
                    }

                    // Populate text entries
                    while (!reader.EndOfStream)
                    {
                        string line = reader.ReadLine();
                        string[] fields = ParseCSVLine(line);
                        if (fields.Length > 0)
                        {
                            TextEntry entry = new TextEntry { TextID = fields[0] };
                            for (int i = 1; i < fields.Length; i++)
                            {
                                Translation translation = new Translation
                                {
                                    LanguageID = headers[i],
                                    TranslatedText = fields[i]
                                };
                                entry.Translations.Add(translation);
                            }
                            TextEntries.Add(entry);
                        }
                    }
                }

                PopulateExternalTranslations();
            }
            catch (Exception e)
            {
                Debug.LogError($"Error loading CSV language file: {e.Message}");
            }
        }

        /// <summary>
        /// Populates the externalTranslations dictionary based on the current language ID.
        /// </summary>
        private void PopulateExternalTranslations()
        {
            externalTranslations.Clear();
            foreach (TextEntry entry in TextEntries)
            {
                Translation translation = entry.Translations.Find(t => t.LanguageID.Equals(currentLanguageID, StringComparison.OrdinalIgnoreCase));
                if (translation != null)
                {
                    externalTranslations[entry.TextID] = translation.TranslatedText;
                }
                else
                {
                    Debug.LogWarning($"Translation for LanguageID '{currentLanguageID}' not found in TextEntry '{entry.TextID}'.");
                }
            }
        }      

        /// <summary>
        /// Exports language data to a JSON file.
        /// </summary>
        /// <param name="filePath">The file path to save the JSON data.</param>
        public void ExportLanguagesToJSON(string filePath)
        {
            try
            {
                LanguageDataWrapper database = new LanguageDataWrapper
                {
                    Languages = this.Languages,
                    TextEntries = this.TextEntries
                };
                string json = JsonUtility.ToJson(database, true);
                File.WriteAllText(filePath, json);

                Debug.Log("Exported to JSON successfully.");
            }
            catch (System.Exception e)
            {
                Debug.LogError("Error exporting to JSON: " + e.Message);
            }
        }

        /// <summary>
        /// Imports language data from a JSON file.
        /// </summary>
        /// <param name="filePath">The file path to import the JSON data from.</param>
        public void ImportLanguagesFromJSON(string filePath)
        {
            try
            {
                if (File.Exists(filePath))
                {
                    string json = File.ReadAllText(filePath);
                    LanguageDataWrapper database = JsonUtility.FromJson<LanguageDataWrapper>(json);
                    this.Languages = database.Languages;
                    this.TextEntries = database.TextEntries;

                    Debug.Log("Imported from JSON successfully.");
                }
                else
                {
                    Debug.LogError("File not found: " + filePath);
                }
            }
            catch (System.Exception e)
            {
                Debug.LogError("Error importing from JSON: " + e.Message);
            }
        }

        /// <summary>
        /// Exports language data to a CSV file.
        /// </summary>
        public void ExportLanguagesToCSV(string filePath)
        {
            try
            {
                using (StreamWriter writer = new StreamWriter(filePath))
                {
                    // Write header
                    writer.Write("textID");
                    foreach (Language language in Languages)
                    {
                        writer.Write($",{language.LanguageID}");
                    }
                    writer.WriteLine();

                    // Write text entries
                    foreach (TextEntry entry in TextEntries)
                    {
                        writer.Write(entry.TextID);
                        foreach (Language language in Languages)
                        {
                            string translatedText = "";
                            Translation translation = entry.Translations.Find(t => t.LanguageID == language.LanguageID);
                            if (translation != null)
                            {
                                translatedText = translation.TranslatedText;
                            }
                            writer.Write($",\"{translatedText}\"");
                        }
                        writer.WriteLine();
                    }
                }

                Debug.Log("Exported to CSV successfully.");
            }
            catch (System.Exception e)
            {
                Debug.LogError("Error exporting to CSV: " + e.Message);
            }
        }

        /// <summary>
        /// Imports language data from a CSV file.
        /// </summary>
        public void ImportLanguagesFromCSV(string filePath)
        {
            try
            {
                if (File.Exists(filePath))
                {
                    using (StreamReader reader = new StreamReader(filePath))
                    {
                        string headerLine = reader.ReadLine();
                        string[] headers = ParseCSVLine(headerLine);

                        // Clear existing languages and text entries
                        Languages.Clear();
                        TextEntries.Clear();

                        // The first column is textID, the rest are LanguageIDs
                        for (int i = 1; i < headers.Length; i++)
                        {
                            Language language = new Language
                            {
                                LanguageID = headers[i],
                                LanguageTitle = headers[i]
                            };
                            Languages.Add(language);
                        }

                        // Read text entries
                        while (!reader.EndOfStream)
                        {
                            string line = reader.ReadLine();
                            string[] fields = ParseCSVLine(line);
                            if (fields.Length > 0)
                            {
                                TextEntry entry = new TextEntry
                                {
                                    TextID = fields[0]
                                };
                                for (int i = 1; i < fields.Length; i++)
                                {
                                    Translation translation = new Translation
                                    {
                                        LanguageID = headers[i],
                                        TranslatedText = fields[i]
                                    };
                                    entry.Translations.Add(translation);
                                }
                                TextEntries.Add(entry);
                            }
                        }
                    }

                    Debug.Log("Imported from CSV successfully.");
                }
                else
                {
                    Debug.LogError("File not found: " + filePath);
                }
            }
            catch (System.Exception e)
            {
                Debug.LogError("Error importing from CSV: " + e.Message);
            }
        }
        /// <summary>
        /// Exports the current language data to an XML file.
        /// </summary>
        /// <param name="filePath">The file path where the XML data will be saved.</param>
        public void ExportLanguagesToXML(string filePath)
        {
            try
            {
                LanguageDataWrapper dataWrapper = new LanguageDataWrapper
                {
                    Languages = this.Languages,
                    TextEntries = this.TextEntries
                };

                XmlSerializer serializer = new XmlSerializer(typeof(LanguageDataWrapper));

                using (FileStream stream = new FileStream(filePath, FileMode.Create))
                {
                    serializer.Serialize(stream, dataWrapper);
                }

                Debug.Log("Exported to XML successfully.");
            }
            catch (Exception e)
            {
                Debug.LogError("Error exporting to XML: " + e.Message);
            }
        }
        /// <summary>
        /// Imports language data from an XML file.
        /// </summary>
        /// <param name="filePath">The file path to import the XML data from.</param>
        /// <summary>
        /// Imports language data from an XML file.
        /// </summary>
        /// <param name="filePath">The file path to import the XML data from.</param>
        public void ImportLanguagesFromXML(string filePath)
        {
            try
            {
                if (File.Exists(filePath))
                {
                    // Initialize the XML serializer with the type LanguageDataWrapper
                    XmlSerializer serializer = new XmlSerializer(typeof(LanguageDataWrapper));

                    // Open the file stream for reading
                    using (FileStream stream = new FileStream(filePath, FileMode.Open))
                    {
                        // Deserialize the XML data into a LanguageDataWrapper object
                        LanguageDataWrapper dataWrapper = (LanguageDataWrapper)serializer.Deserialize(stream);

                        // Update the internal lists with the imported data
                        this.Languages = dataWrapper.Languages;
                        this.TextEntries = dataWrapper.TextEntries;

                        // Update the externalTranslations dictionary based on the currentLanguageID
                        externalTranslations.Clear();
                        foreach (TextEntry entry in TextEntries)
                        {
                            Translation translation = entry.Translations.Find(t => t.LanguageID.Equals(currentLanguageID, StringComparison.OrdinalIgnoreCase));
                            if (translation != null)
                            {
                                externalTranslations[entry.TextID] = translation.TranslatedText;
                            }
                            else
                            {
                                Debug.LogWarning($"Translation for LanguageID '{currentLanguageID}' not found in TextEntry '{entry.TextID}'.");
                            }
                        }
                    }

                    Debug.Log("Imported from XML successfully.");
                }
                else
                {
                    Debug.LogError("File not found: " + filePath);
                }
            }
            catch (Exception e)
            {
                Debug.LogError("Error importing from XML: " + e.Message);
            }
        }


        /// <summary>
        /// Exports an empty base model for translations in the selected format.
        /// </summary>
        /// <param name="format">The format to export (JSON, CSV, XML).</param>
        /// <param name="filePath">The file path to save the base model.</param>
        public void ExportBaseModel(string format, string filePath)
        {
            try
            {
                if (string.IsNullOrEmpty(filePath))
                {
                    Debug.LogError("File path is not set.");
                    return;
                }

                if (format == "JSON")
                {
                    // Export empty structures to JSON
                    string jsonContent = JsonUtility.ToJson(new LanguageDataWrapper
                    {
                        Languages = this.Languages,
                        TextEntries = new List<TextEntry>() // Empty list
                    }, true);
                    File.WriteAllText(filePath, jsonContent);
                }
                else if (format == "CSV")
                {
                    // Export CSV file with headers including languages
                    using (StreamWriter writer = new StreamWriter(filePath))
                    {
                        // Write header with textID and LanguageIDs
                        writer.Write("textID");
                        foreach (Language language in Languages)
                        {
                            writer.Write($",{language.LanguageID}");
                        }
                        writer.WriteLine();

                        // No text entries, as it's a base model
                    }
                }
                else if (format == "XML")
                {
                    // Export empty structures to XML
                    LanguageDataWrapper dataWrapper = new LanguageDataWrapper
                    {
                        Languages = this.Languages,
                        TextEntries = new List<TextEntry>() // Empty list
                    };
                    XmlSerializer serializer = new XmlSerializer(typeof(LanguageDataWrapper));
                    using (FileStream stream = new FileStream(filePath, FileMode.Create))
                    {
                        serializer.Serialize(stream, dataWrapper);
                    }
                }

                Debug.Log("Base model export completed.");
            }
            catch (System.Exception e)
            {
                Debug.LogError("Error exporting base model: " + e.Message);
            }
        }

        /// <summary>
        /// Helper method to parse CSV lines considering possible commas within quotes.
        /// </summary>
        /// <param name="line">The CSV line to parse.</param>
        /// <returns>Array of parsed strings.</returns>
        private string[] ParseCSVLine(string line)
        {
            List<string> result = new List<string>();
            bool inQuotes = false;
            string value = "";

            foreach (char c in line)
            {
                if (c == '\"')
                {
                    inQuotes = !inQuotes;
                }
                else if (c == ',' && !inQuotes)
                {
                    result.Add(value);
                    value = "";
                }
                else
                {
                    value += c;
                }
            }
            result.Add(value);

            return result.ToArray();
        }

        /// <summary>
        /// Retrieves the translated text for a given text ID based on the selected language.
        /// If the text ID is not found or the translation for the current language does not exist, returns an empty string.
        /// If enableTextFormatting is true, the returned text is processed for escape characters and formatting markers.
        /// </summary>
        /// <param name="textID">The text ID to retrieve.</param>
        /// <returns>The translated text for the provided text ID in the current language.</returns>
        public string GetTextEntryByID(string textID)
        {
            string result = string.Empty;

            if (languageMode == LanguageMode.InternalDatabase)
            {
                TextEntry entry = TextEntries.Find(e => e.TextID.Equals(textID, StringComparison.OrdinalIgnoreCase));
                if (entry != null)
                {
                    Translation translation = entry.Translations.Find(t => t.LanguageID.Equals(currentLanguageID, StringComparison.OrdinalIgnoreCase));
                    if (translation != null)
                    {
                        result = translation.TranslatedText;
                    }
                    else
                    {
                        Debug.LogWarning($"Translation for LanguageID '{currentLanguageID}' not found in TextEntry '{textID}'.");
                    }
                }
                else
                {
                    Debug.LogWarning($"TextEntry with TextID '{textID}' not found.");
                }
            }
            else
            {
                if (externalTranslations.ContainsKey(textID))
                {
                    result = externalTranslations[textID];
                }
                else
                {
                    Debug.LogWarning($"External translation for TextID '{textID}' not found.");
                }
            }

            // Process text formatting if enableTextFormatting is true
            if (enableTextFormatting)
            {
                result = ProcessTextFormatting(result);
            }

            return result;
        }

        /// <summary>
        /// Processes the input text by converting common escape sequences and custom formatting markers 
        /// to their corresponding display characters or tags.
        /// For example, converts "\\n" to a newline, "[b]...[/b]" to "<b>...</b>", etc.
        /// </summary>
        /// <param name="input">The raw input text to process.</param>
        /// <returns>The processed text with formatting applied.</returns>
        private string ProcessTextFormatting(string input)
        {
            if (string.IsNullOrEmpty(input))
                return input;

            input = input.Replace("\\n", "\n");
            input = input.Replace("\\t", "\t");
            input = input.Replace("\\r", "\r");
            input = input.Replace("\\\"", "\"");
            input = input.Replace("\\\\", "\\");

            input = input.Replace("[b]", "<b>").Replace("[/b]", "</b>");
            input = input.Replace("[i]", "<i>").Replace("[/i]", "</i>");

            return input;
        }


        /// <summary>
        /// Gets the current language ID.
        /// </summary>
        /// <returns>The current language ID.</returns>
        public string GetCurrentLanguage()
        {
            return currentLanguageID;
        }
    }
}
