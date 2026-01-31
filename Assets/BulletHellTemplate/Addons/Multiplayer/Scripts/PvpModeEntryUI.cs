using TMPro;
using UnityEngine;
using UnityEngine.UI;
using BulletHellTemplate.PVP;

namespace BulletHellTemplate
{
    public class PvpModeEntryUI : MonoBehaviour
    {
        public Image icon;
        public TextMeshProUGUI title;
        public Button selectButton;

        private PvpModeData _data;
        private System.Action<PvpModeData> _onSelected;

        public void Setup(PvpModeData data, System.Action<PvpModeData> onSelected)
        {
            _data = data;
            _onSelected = onSelected;
            if (icon) icon.sprite = data.iconPreview;
            if (title) title.text = data.battleName;
            if (selectButton)
            {
                selectButton.onClick.RemoveAllListeners();
                selectButton.onClick.AddListener(() => _onSelected?.Invoke(_data));
            }
        }
    }
}
