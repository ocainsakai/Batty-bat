using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;

namespace BulletHellTemplate
{
    public class FavouriteCharacter : MonoBehaviour
    {
        private int characterId;

        /// <summary>
        /// Sets the character ID that will be saved as the favorite.
        /// </summary>
        /// <param name="_characterId">The ID of the character.</param>
        public void SetCharacterId(int _characterId)
        {
            characterId = _characterId;
        }

        /// <summary>
        /// Called when the user clicks to set the favorite character.
        /// Saves the favorite character locally and in the database.
        /// </summary>
        public async void OnClickSetFavourite()
        {
            RequestResult ok = await BackendManager.Service.UpdatePlayerCharacterFavouriteAsync(characterId);

            if(ok.Success)
            {
                UICharacterMenu.Singleton.UpdateFavouriteSelected(characterId);
                return;
            }
            Debug.LogError($"Failed to update favourite character");          
        }

    }
}
