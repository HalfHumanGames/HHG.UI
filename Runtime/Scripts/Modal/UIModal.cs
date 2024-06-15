using TMPro;
using UnityEngine;

namespace HHG.UISystem.Runtime
{
    public class UIModal : UI<Modal>
    {
        [SerializeField] private TextMeshProUGUI headerLabel;
        [SerializeField] private TextMeshProUGUI descriptionLabel;
        [SerializeField] private UIButton[] buttons;

        public override void Refresh(Modal modal)
        {
            headerLabel.text = modal.Header;
            descriptionLabel.text = modal.Description;

            for (int i = 0; i < buttons.Length; i++)
            {
                UIButton button = buttons[i];

                if (i < modal.Buttons.Length)
                {
                    ModalButton modalButton = modal.Buttons[i];
                    button.GetComponentInChildren<TextMeshProUGUI>().text = modal.Buttons[i].Text;
                    button.OnClick.Actions.Clear();
                    button.OnClick.Actions.AddRange(modalButton.OnClick.Actions);
                    button.gameObject.SetActive(true);
                }
                else
                {
                    button.gameObject.SetActive(false);
                }
            }

            EnableBack(modal.BackEnabled);
        }
    }
}