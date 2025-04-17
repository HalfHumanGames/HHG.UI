using HHG.Common.Runtime;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;

namespace HHG.UI.Runtime
{
    public class UIPauseMenu : UI<PauseMenuData>
    {
        [SerializeField] protected SelectableNavigation navigation;
        [SerializeField] protected List<UIButton> buttons = new List<UIButton>();

        private float previousTimeScale = 1f;

        public override void Refresh(PauseMenuData data)
        {
            base.Refresh(data);

            buttons.Resize(data.Buttons.Count);
            buttons.Select(b => b.Button).SetNavigation(navigation);

            for (int i = 0; i < buttons.Count; i++)
            {
                UIButton button = buttons[i];
                button.Label.text = data.Buttons[i].Text;
                button.Button.interactable = true;
                button.OnClick.RemoveListener(OnButtonClick);
                button.OnClick.AddListener(OnButtonClick);
            }
        }

        private void OnButtonClick(UIButton button)
        {
            int index = button.transform.GetSiblingIndex();
            ButtonData modalButton = data.Buttons[index];
            modalButton.OnClick.Invoke(button, this);
        }

        protected override void OnOpen()
        {
            base.OnOpen();

            previousTimeScale = Time.timeScale;
            Time.timeScale = 0f;
        }

        protected override void OnClose()
        {
            base.OnClose();

            Time.timeScale = previousTimeScale;
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