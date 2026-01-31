using TMPro;
using UnityEngine;
using UnityEngine.UI;
using Cysharp.Threading.Tasks;
using System;

namespace BulletHellTemplate
{
    public class KillFeedEntryUI : MonoBehaviour
    {
        public Image killerIcon;
        public TextMeshProUGUI killerNick;
        public Image victimIcon;
        public TextMeshProUGUI victimNick;
        public CanvasGroup group;
        public float lifetimeSec = 3f;

        public void Setup(Sprite killer, string kNick, Sprite victim, string vNick)
        {
            if (killerIcon) killerIcon.sprite = killer;
            if (killerNick) killerNick.text = kNick;
            if (victimIcon) victimIcon.sprite = victim;
            if (victimNick) victimNick.text = vNick;

            FadeAndDestroyAsync().Forget();
        }

        private async UniTaskVoid FadeAndDestroyAsync()
        {
            if (!group) { await UniTask.Delay(TimeSpan.FromSeconds(lifetimeSec)); Destroy(gameObject); return; }

            group.alpha = 1f;
            await UniTask.Delay(TimeSpan.FromSeconds(lifetimeSec - 0.3f));
            float t = 0f;
            while (t < 0.3f)
            {
                t += Time.deltaTime;
                group.alpha = 1f - (t / 0.3f);
                await UniTask.Yield();
            }
            Destroy(gameObject);
        }
    }
}
