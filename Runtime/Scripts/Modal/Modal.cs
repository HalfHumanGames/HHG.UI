using System.Collections.Generic;
using UnityEngine;

namespace HHG.UI.Runtime
{
    [System.Serializable]
    public class Modal
    {
        public string Header;
        [TextArea] public string Description;
        public bool BackEnabled = true;
        public List<ModalButton> Buttons = new List<ModalButton>();
        public int Selection;

        public Modal()
        {

        }

        public Modal(string header, string description, bool backEnabled, List<ModalButton> buttons)
        {
            Header = header;
            Description = description;
            BackEnabled = backEnabled;
            Buttons = buttons;
        }

        public Modal(string header, string description, bool backEnabled, IEnumerable<ModalButton> buttons)
        {
            Header = header;
            Description = description;
            BackEnabled = backEnabled;
            Buttons.AddRange(buttons);
        }
    }
}