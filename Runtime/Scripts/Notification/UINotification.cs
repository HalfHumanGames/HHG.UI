using HHG.Common.Runtime;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace HHG.UISystem.Runtime
{
    public class UINotification : UI<Notification>
    {
        [SerializeField] private float duration = 3f;
        [SerializeField] private TextMeshProUGUI label;
        [SerializeField] private Image background;

        public override void Refresh(Notification notification)
        {
            label.text = notification.Text;

            if (notification.SetTextColor)
            {
                label.color = notification.TextColor;
            }

            if (notification.SetBackgroundColor)
            {
                background.color = notification.BackgroundColor;
            }

            this.AfterRealtime(duration, _ => Close());
        }
    }
}