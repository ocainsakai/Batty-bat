using System;
using System.Collections.Generic;

namespace LanguageManager
{
    /// <summary>
    /// Wrapper class to encapsulate language data for serialization and deserialization.
    /// </summary>
    [Serializable]
    public class LanguageDataWrapper
    {
        public List<Language> Languages;
        public List<TextEntry> TextEntries;
    }
}
