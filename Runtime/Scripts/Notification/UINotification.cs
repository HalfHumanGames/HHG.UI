using HHG.Common.Runtime;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace HHG.UISystem.Runtime
{
    public class UINotification : UI<Notification>
    {
        [SerializeField] private TextMeshProUGUI label;
        [SerializeField] private Image background;

        public override void Refresh(Notification notification)
        {
            label.text = notification.Text;
            label.color = notification.TextColor;
            background.color = notification.BackgroundColor;

            this.Invoker().AfterRealtime(3f, _ => Close());
        }
    }
}