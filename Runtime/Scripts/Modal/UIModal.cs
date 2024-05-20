using TMPro;
using UnityEngine;

namespace HHG.UISystem.Runtime
{
    public class UIModal : UIView<Modal>
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
                    button.Actions.Clear();
                    button.Actions.AddRange(modalButton.Actions);
                    button.ActionsAsync.Clear();
                    button.ActionsAsync.AddRange(modalButton.ActionsAsync);
                    button.gameObject.SetActive(true);
                }
                else
                {
                    button.gameObject.SetActive(false);
                }
            }
        }
    }
}