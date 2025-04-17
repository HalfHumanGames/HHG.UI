using System.Collections.Generic;

namespace HHG.UI.Runtime
{
    [System.Serializable]
    public class PauseMenuData
    {
        public List<ButtonData> Buttons = new List<ButtonData>();

        public PauseMenuData()
        {

        }

        public PauseMenuData(List<ButtonData> buttons)
        {
            Buttons = buttons;
        }

        public PauseMenuData(IEnumerable<ButtonData> buttons)
        {
            Buttons.AddRange(buttons);
        }
    }
}