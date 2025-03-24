using HHG.Common.Runtime;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEditor;
using UnityEngine;
using UnityEngine.EventSystems;

namespace HHG.UI.Runtime
{
    public class UIModal : UIModal<Modal>
    {
        
    }

    public class UIModal<T> : UI<T> where T : Modal
    {
        public int Selection => selection;

        [SerializeField] protected SelectableNavigation navigation;
        [SerializeField] protected TextMeshProUGUI headerLabel;
        [SerializeField] protected TextMeshProUGUI descriptionLabel;
        [SerializeField] protected TextMeshProUGUI buttonDescriptionLabel;
        [SerializeField] protected UIButton closeButton;
        [SerializeField] protected List<UIButton> buttons = new List<UIButton>();

        private List<EventTrigger.Entry> oldEntries = new List<EventTrigger.Entry>();
        private List<EventTrigger.Entry> newEntries = new List<EventTrigger.Entry>();

        private int selection;

        public override void Refresh(T data)
        {
            base.Refresh(data);

            headerLabel.text = data.Header;
            descriptionLabel.text = data.Description;
            descriptionLabel.gameObject.SetActive(!string.IsNullOrEmpty(data.Description));
            buttonDescriptionLabel.text = string.Empty;
            buttonDescriptionLabel.gameObject.SetActive(false);
            closeButton.gameObject.SetActive(data.BackEnabled);

            buttons.Resize(data.Buttons.Count);
            buttons.Select(b => b.Button).SetNavigation(navigation);

            for (int i = 0; i < buttons.Count; i++)
            {
                UIButton button = buttons[i];
                button.Label.text = data.Buttons[i].Text;
                button.Button.interactable = true;
                button.OnClick.RemoveListener(OnButtonClick);
                button.OnClick.AddListener(OnButtonClick);

                string optionDescription = data.Buttons[i].Description;

                if (buttons[i].TryGetComponent(out EventTrigger eventTrigger))
                {
                    eventTrigger.triggers.RemoveRange(oldEntries);

                    newEntries.Add(eventTrigger.AddTrigger(EventTriggerType.Select, () =>
                    {
                        buttonDescriptionLabel.text = optionDescription;
                        buttonDescriptionLabel.gameObject.SetActive(!string.IsNullOrEmpty(optionDescription));
                    }));

                    newEntries.Add(eventTrigger.AddTrigger(EventTriggerType.PointerEnter, () =>
                    {
                        buttonDescriptionLabel.text = optionDescription;
                        buttonDescriptionLabel.gameObject.SetActive(!string.IsNullOrEmpty(optionDescription));
                    }));
                }

                if (i == 0)
                {
                    buttonDescriptionLabel.text = optionDescription;
                    buttonDescriptionLabel.gameObject.SetActive(!string.IsNullOrEmpty(optionDescription));
                }
            }

            EnableBack(data.BackEnabled);
        }

        private void OnButtonClick(UIButton button)
        {
            selection = button.transform.GetSiblingIndex();
            ModalButton modalButton = data.Buttons[selection];
            modalButton.OnClick.Invoke(button, this);
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