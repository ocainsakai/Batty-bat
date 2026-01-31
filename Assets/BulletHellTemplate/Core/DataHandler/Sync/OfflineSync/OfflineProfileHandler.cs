namespace BulletHellTemplate
{
    /// <summary>
    /// Handles player-profile operations (nickname, icon, frame…) in the offline backend.
    /// </summary>
    public static class OfflineProfileHandler
    {     
        public static RequestResult ChangePlayerName( string newName)
        {
            var minLen = GameInstance.Singleton.minNameLength;
            var maxLen = GameInstance.Singleton.maxNameLength;
            var needTicket = GameInstance.Singleton.needTicket;
            var ticketCurrency = GameInstance.Singleton.changeNameTick;
            var ticketsRequired = GameInstance.Singleton.ticketsToChange;

            if (string.IsNullOrEmpty(newName) || newName.Length < minLen || newName.Length > maxLen)
                return RequestResult.Fail("0");

            if (PlayerSave.GetPlayerName().Equals(newName, System.StringComparison.OrdinalIgnoreCase))
                return RequestResult.Fail("2");

            if (needTicket)
            {
                int balance = MonetizationManager.GetCurrency(ticketCurrency);
                if (balance < ticketsRequired)
                    return RequestResult.Fail("1");

                MonetizationManager.SetCurrency(ticketCurrency, balance - ticketsRequired, pushToBackend: false);
            }

            // Apply
            PlayerSave.SetPlayerName(newName);
            return RequestResult.Ok();
        }
       
        public static RequestResult ChangePlayerIcon(string iconId)
        {
            IconItem item = GameInstance.Singleton.GetIconItemById(iconId);
                                  
            if (item == null) return RequestResult.Fail("1");

            bool owned = item.isUnlocked || PlayerSave.IsIconPurchased(iconId);
            if (!owned) return RequestResult.Fail("0");

            PlayerSave.SetPlayerIcon(iconId);
            return RequestResult.Ok();
        }
      
        public static RequestResult ChangePlayerFrame(string frameId)
        {
            FrameItem item = GameInstance.Singleton.GetFrameItemById(frameId);
                                   
            if (item == null) return RequestResult.Fail("1");

            bool owned = item.isUnlocked || PlayerSave.IsFramePurchased(frameId);
            if (!owned) return RequestResult.Fail("0");

            PlayerSave.SetPlayerFrame(frameId);
            return RequestResult.Ok();
        }

    }
}
