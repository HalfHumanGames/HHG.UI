using HHG.Common.Runtime;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEditor;
using UnityEngine;
using UnityEngine.EventSystems;

namespace HHG.UI.Runtime
{
    public class UIModal : UIModal<ModalData>
    {
        
    }

    public class UIModal<T> : UI<T> where T : ModalData
    {
        public int Selection => selection;

        [SerializeField] protected SelectableNavigation navigation;
        [SerializeField] protected TMP_Text headerLabel;
        [SerializeField] protected TMP_Text descriptionLabel;
        [SerializeField] protected TMP_Text buttonDescriptionLabel;
        [SerializeField] protected UIButton closeButton;
        [SerializeField] protected List<UIButton> buttons = new List<UIButton>();

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
                string text = data.Buttons[i].Text;
                UIButton button = buttons[i];
                button.name = $"Button - {text}";
                button.Label.text = text;
                button.Button.interactable = true;
                button.OnClick.RemoveListener(OnButtonClick);
                button.OnClick.AddListener(OnButtonClick);

                string optionDescription = data.Buttons[i].Description;

                if (buttons[i].TryGetComponent(out EventTrigger eventTrigger))
                {
                    eventTrigger.RemoveTrigger(EventTriggerType.Select, OnButtonSelect);
                    eventTrigger.RemoveTrigger(EventTriggerType.PointerEnter, OnButtonPointerEnter);
                    eventTrigger.AddTrigger(EventTriggerType.Select, OnButtonSelect);
                    eventTrigger.AddTrigger(EventTriggerType.PointerEnter, OnButtonPointerEnter);
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
            ButtonData modalButton = data.Buttons[selection];
            modalButton.OnClick.Invoke(button, this);
        }

        private void OnButtonSelect(BaseEventData eventData)
        {
            UpdateButtonDescriptionLabel(eventData.selectedObject);
        }

        private void OnButtonPointerEnter(BaseEventData eventData)
        {
            UpdateButtonDescriptionLabel((eventData as PointerEventData).pointerEnter);
        }

        private void UpdateButtonDescriptionLabel(GameObject selectedObject)
        {
            int index = selectedObject.transform.GetSiblingIndex();
            string optionDescription = data.Buttons[index].Description;
            buttonDescriptionLabel.text = optionDescription;
            buttonDescriptionLabel.gameObject.SetActive(!string.IsNullOrEmpty(optionDescription));
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