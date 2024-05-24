using TMPro;
using UnityEngine;
using HHG.Common.Runtime;

namespace HHG.UISystem.Runtime
{
    public class UINotification : UI<Notification>
    {
        [SerializeField] private TextMeshProUGUI label;
        [SerializeField] private TextMeshProUGUI background;

        public override void Refresh(Notification notification)
        {
            label.text = notification.Text;
            label.color = notification.TextColor;
            background.color = notification.BackgroundColor;

            this.Invoker().After(3f, _ => Close());
        }
    }
}