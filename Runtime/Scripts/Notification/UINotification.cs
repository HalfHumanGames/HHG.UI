using HHG.Common.Runtime;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace HHG.UI.Runtime
{
    public class UINotification : UI<Notification>
    {
        [SerializeField] private float duration = 3f;
        [SerializeField] private TextMeshProUGUI label;
        [SerializeField] private Image background;

        public override void Refresh(Notification data)
        {
            base.Refresh(data);

            label.text = data.Text;

            if (data.SetTextColor)
            {
                label.color = data.TextColor;
            }

            if (data.SetBackgroundColor)
            {
                background.color = data.BackgroundColor;
            }

            this.AfterRealtime(duration, _ => Close());
        }
    }
}