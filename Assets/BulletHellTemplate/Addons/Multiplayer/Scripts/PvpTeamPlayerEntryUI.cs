using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace BulletHellTemplate
{
    public class PvpTeamPlayerEntryUI : MonoBehaviour
    {
        public Image icon;
        public TextMeshProUGUI nickname;
        public Image hpBar;
        public TextMeshProUGUI deadCountdown;

        public void Setup(Sprite _icon, string _nick)
        {
            if (icon) icon.sprite = _icon;
            if (nickname) nickname.text = _nick;
            SetHP(1f);
            SetDeadCountdown(-1);
        }

        public void SetHP(float normalized) => hpBar.fillAmount = Mathf.Clamp01(normalized);
        public void SetDeadCountdown(int secs)
        {
            if (!deadCountdown) return;
            deadCountdown.text = secs > 0 ? secs.ToString() : "";
        }
    }
}
