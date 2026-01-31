using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace BulletHellTemplate
{
    public class UISkillInfo : MonoBehaviour
    {
        public Image icon;
        public TextMeshProUGUI title;
        public TextMeshProUGUI description;

        /// <summary>
        /// Set Skill info on menu.
        /// </summary>
        public void SetSkillInfo(Sprite _icon, string _title, string _description)
        {
            icon.sprite = _icon;
            title.text = _title;
            description.text = _description;
        }
    }
}
