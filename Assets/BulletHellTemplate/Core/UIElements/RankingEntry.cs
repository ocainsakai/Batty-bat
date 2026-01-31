using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace BulletHellTemplate
{
    public class RankingEntry : MonoBehaviour
    {
        [Header("UI Elements")]
        [Tooltip("Image displaying the player's icon.")]
        public Image playerIcon;

        [Tooltip("Image displaying the player's frame.")]
        public Image playerFrame;

        [Tooltip("Image indicating if this entry is selected.")]
        public Image selected;

        [Tooltip("Text displaying the player's name.")]
        public TextMeshProUGUI playerName;

        [Tooltip("Text displaying the player's ranking position.")]
        public TextMeshProUGUI ranking;

        [Tooltip("Text displaying the player's score.")]
        public TextMeshProUGUI score;

        [Tooltip("GameObject for first place icon.")]
        public GameObject firstPlace;

        [Tooltip("GameObject for second place icon.")]
        public GameObject secondPlace;

        [Tooltip("GameObject for third place icon.")]
        public GameObject thirdPlace;

        [HideInInspector]
        /// <summary>
        /// The ID of the player's favorite character.
        /// </summary>
        public int characterFavourite;

        /// <summary>
        /// Sets the ranking information for a player.
        /// </summary>
        /// <param name="_playerName">The name of the player.</param>
        /// <param name="_score">The player's score.</param>
        /// <param name="rankingPosition">The player's position in the ranking.</param>
        /// <param name="playerIconId">The ID of the player's icon.</param>
        /// <param name="playerFrameId">The ID of the player's frame.</param>
        /// <param name="playerCharacterFavourite">The ID of the player's favorite character.</param>
        public void SetRankingInfo(string _playerName, string _score, int rankingPosition, Sprite playerIconId, Sprite playerFrameId, int playerCharacterFavourite)
        {
            playerName.text = _playerName;
            score.text = _score;
            ranking.text = rankingPosition.ToString();
            characterFavourite = playerCharacterFavourite;
            playerIcon.sprite = playerIconId;
            playerFrame.sprite = playerFrameId;

            firstPlace.SetActive(rankingPosition == 1);
            secondPlace.SetActive(rankingPosition == 2);
            thirdPlace.SetActive(rankingPosition == 3);
        }

        /// <summary>
        /// Sets the selection state of this ranking entry.
        /// </summary>
        /// <param name="isSelected">True if selected, false otherwise.</param>
        public void SetSelected(bool isSelected)
        {
            selected.gameObject.SetActive(isSelected);
        }

        /// <summary>
        /// Handles the click event to select this ranking entry.
        /// </summary>
        public void OnClickSetFavouriteCharacter()
        {
            UIRankingMenu.Singleton.OnRankingEntrySelected(this);
        }
    }
}
