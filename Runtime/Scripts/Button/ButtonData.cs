using HHG.Common.Runtime;
using UnityEngine.Serialization;

namespace HHG.UI.Runtime
{
    [System.Serializable]
    public class ButtonData
    {
        [FormerlySerializedAs("text")] public string Text;
        [FormerlySerializedAs("text")] public string Description;
        [FormerlySerializedAs("onClick")] public ActionEvent<UIButton, UI> OnClick = new ActionEvent<UIButton, UI>();

        public ButtonData() { }
        public ButtonData(string text, System.Action onClick) : this(text, string.Empty, new ActionEvent<UIButton, UI>(onClick)) { }
        public ButtonData(string text, System.Action<UIButton> onClick) : this(text, string.Empty, new ActionEvent<UIButton, UI>(onClick)) { }
        public ButtonData(string text, System.Action<UIButton, UI> onClick) : this(text, string.Empty, new ActionEvent<UIButton, UI>(onClick)) { }
        public ButtonData(string text, string description, System.Action onClick) : this(text, description, new ActionEvent<UIButton, UI>(onClick)) { }
        public ButtonData(string text, string description, System.Action<UIButton> onClick) : this(text, description, new ActionEvent<UIButton, UI>(onClick)) { }
        public ButtonData(string text, string description, System.Action<UIButton, UI> onClick) : this(text, description, new ActionEvent<UIButton, UI>(onClick)) { }
        public ButtonData(string text, ActionEvent<UIButton, UI> onClick) : this(text, string.Empty, onClick) { }
        public ButtonData(string text, string description, ActionEvent<UIButton, UI> onClick)
        {
            Text = text;
            Description = description;
            OnClick = onClick;
        }
    }
}