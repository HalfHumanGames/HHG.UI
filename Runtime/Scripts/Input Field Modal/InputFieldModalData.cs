using System.Collections.Generic;

namespace HHG.UI.Runtime
{
    [System.Serializable]
    public class InputFieldModalData : ModalData
    {
        public string InputFieldText;

        public InputFieldModalData() : base()
        {

        }

        public InputFieldModalData(string header, string description, string inputFieldText, bool backEnabled, List<ButtonData> buttons) : base(header, description, backEnabled, buttons)
        {
            InputFieldText = inputFieldText;
        }

        public InputFieldModalData(string header, string description, string inputFieldText, bool backEnabled, IEnumerable<ButtonData> buttons) : base(header, description, backEnabled, buttons)
        {
            InputFieldText = inputFieldText;
        }
    }
}