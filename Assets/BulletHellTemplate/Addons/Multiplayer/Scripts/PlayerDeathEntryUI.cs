using UnityEngine;
using TMPro;
using UnityEngine.UI;

namespace BulletHellTemplate
{
    public class PlayerDeathEntryUI : MonoBehaviour
    {
        public Image icon;
        public TMP_Text nickname;
        public TMP_Text countdown;

        public void Setup(Sprite ic, string name, int secs)
        {
            if (icon) icon.sprite = ic;
            if (nickname) nickname.text = name ?? "";
            SetSeconds(secs);
        }
        public void SetSeconds(int secs)
        {
            if (countdown) countdown.text = Mathf.Max(0, secs).ToString();
        }
    }
}