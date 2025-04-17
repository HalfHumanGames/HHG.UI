using System.Collections.Generic;
using UnityEngine;

namespace HHG.UI.Runtime
{
    [System.Serializable]
    public class ModalData
    {
        public string Header;
        [TextArea] public string Description;
        public bool BackEnabled = true;
        public List<ButtonData> Buttons = new List<ButtonData>();

        public ModalData()
        {

        }

        public ModalData(string header, string description, bool backEnabled, List<ButtonData> buttons)
        {
            Header = header;
            Description = description;
            BackEnabled = backEnabled;
            Buttons = buttons;
        }

        public ModalData(string header, string description, bool backEnabled, IEnumerable<ButtonData> buttons)
        {
            Header = header;
            Description = description;
            BackEnabled = backEnabled;
            Buttons.AddRange(buttons);
        }
    }
}