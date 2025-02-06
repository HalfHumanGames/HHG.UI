using HHG.Common.Runtime;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;

namespace HHG.UISystem.Runtime
{
    public class UIModal : UI<Modal>
    {
        [SerializeField] private SelectableNavigation navigation;
        [SerializeField] private TextMeshProUGUI headerLabel;
        [SerializeField] private TextMeshProUGUI descriptionLabel;
        [SerializeField] private List<UIButton> buttons = new List<UIButton>();

        public override void Refresh(Modal modal)
        {
            headerLabel.text = modal.Header;
            descriptionLabel.text = modal.Description;

            for (int i = 0; i < buttons.Count; i++)
            {
                UIButton button = buttons[i];

                if (i < modal.Buttons.Count)
                {
                    ModalButton modalButton = modal.Buttons[i];
                    button.GetComponentInChildren<TextMeshProUGUI>().text = modal.Buttons[i].Text;
                    button.OnClick.RemoveAllListeners();
                    button.OnClick.Invoked += () => modalButton.OnClick.Invoke(button);
                    button.gameObject.SetActive(true);
                }
                else
                {
                    button.gameObject.SetActive(false);
                }
            }

            buttons.Select(b => b.Button).SetNavigation(navigation);

            EnableBack(modal.BackEnabled);
        }

        protected override void OnFocus()
        {
            base.OnFocus();

            if (buttons.Count > 0)
            {
                buttons[0].Button.Select();
            }
        }
    }
}