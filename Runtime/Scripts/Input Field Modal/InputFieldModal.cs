using System.Collections.Generic;

namespace HHG.UI.Runtime
{
    [System.Serializable]
    public class InputFieldModal : Modal
    {
        public string InputFieldText;

        public InputFieldModal() : base()
        {

        }

        public InputFieldModal(string header, string description, string inputFieldText, bool backEnabled, List<ModalButton> buttons) : base(header, description, backEnabled, buttons)
        {
            InputFieldText = inputFieldText;
        }
    }
}