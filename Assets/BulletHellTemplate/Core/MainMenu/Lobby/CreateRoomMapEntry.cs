using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace BulletHellTemplate
{
    [RequireComponent(typeof(Button))]
    public class CreateRoomMapEntry : MonoBehaviour
    {
        [SerializeField] private Image preview;
        [SerializeField] private Image selectedOverlay;
        [SerializeField] private TextMeshProUGUI roomName;
        private int mapId;
        public int MapId => mapId;
        public void Setup(int id, Sprite icon, string _roomName, bool selected)
        {
            mapId = id;
            preview.sprite = icon;
            roomName.text = _roomName;
            SetSelected(selected);
            GetComponent<Button>().onClick.RemoveAllListeners();
            GetComponent<Button>().onClick.AddListener(() => UILobby.Instance.SetCreateRoomMap(mapId));
        }

        public void SetSelected(bool state)
        {
            if (selectedOverlay) selectedOverlay.gameObject.SetActive(state);
        }
    }
}
