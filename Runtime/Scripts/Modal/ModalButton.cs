using HHG.Common.Runtime;
using UnityEngine.Serialization;

namespace HHG.UI.Runtime
{
    [System.Serializable]
    public class ModalButton
    {
        [FormerlySerializedAs("text")] public string Text;
        [FormerlySerializedAs("text")] public string Description;
        [FormerlySerializedAs("onClick")] public ActionEvent<UIButton, UI> OnClick = new ActionEvent<UIButton, UI>();

        public ModalButton() { }
        public ModalButton(string text, System.Action onClick) : this(text, string.Empty, new ActionEvent<UIButton, UI>(onClick)) { }
        public ModalButton(string text, System.Action<UIButton> onClick) : this(text, string.Empty, new ActionEvent<UIButton, UI>(onClick)) { }
        public ModalButton(string text, System.Action<UIButton, UI> onClick) : this(text, string.Empty, new ActionEvent<UIButton, UI>(onClick)) { }
        public ModalButton(string text, string description, System.Action onClick) : this(text, description, new ActionEvent<UIButton, UI>(onClick)) { }
        public ModalButton(string text, string description, System.Action<UIButton> onClick) : this(text, description, new ActionEvent<UIButton, UI>(onClick)) { }
        public ModalButton(string text, string description, System.Action<UIButton, UI> onClick) : this(text, description, new ActionEvent<UIButton, UI>(onClick)) { }
        public ModalButton(string text, ActionEvent<UIButton, UI> onClick) : this(text, string.Empty, onClick) { }
        public ModalButton(string text, string description, ActionEvent<UIButton, UI> onClick)
        {
            Text = text;
            Description = description;
            OnClick = onClick;
        }
    }
}