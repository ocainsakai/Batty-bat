namespace BulletHellTemplate
{
    public static partial class PlayerSave
    {
        // Account keys
        private const string KEY_NAME = "PLAYERNAME_";
        private const string KEY_ICON = "PLAYERICON_";
        private const string KEY_FRAME = "PLAYERFRAME_";
        private const string KEY_ACC_LEVEL = "PLAYERACCOUNTLEVEL_";
        private const string KEY_ACC_CUREXP = "PLAYERACCOUNTCURRENTEXP_";    

        public static void SetPlayerName(string name) =>
            SecurePrefs.SetEncryptedString(KEY_NAME, name);

        public static string GetPlayerName() =>
             SecurePrefs.GetDecryptedString(KEY_NAME, string.Empty);

        public static void SetPlayerIcon(string icon) =>
            SecurePrefs.SetEncryptedString(KEY_ICON, icon);

        public static string GetPlayerIcon() =>
            SecurePrefs.GetDecryptedString(KEY_ICON, GameInstance.Singleton.iconItems[0].iconId);

        public static void SetPlayerFrame(string frame) =>
            SecurePrefs.SetEncryptedString(KEY_FRAME, frame);

        public static string GetPlayerFrame() =>
            SecurePrefs.GetDecryptedString(KEY_FRAME, GameInstance.Singleton.frameItems[0].frameId);

        public static void SetAccountLevel(int level) =>
            SecurePrefs.SetEncryptedInt(KEY_ACC_LEVEL, level);

        public static int GetAccountLevel() =>
            SecurePrefs.GetDecryptedInt(KEY_ACC_LEVEL, 1);

        public static void SetAccountCurrentExp(int exp) =>
            SecurePrefs.SetEncryptedInt(KEY_ACC_CUREXP, exp);

        public static int GetAccountCurrentExp() =>
            SecurePrefs.GetDecryptedInt(KEY_ACC_CUREXP, 0);

    }
}
