using System;
using UnityEngine;
using UnityEngine.Serialization;

namespace HHG.UI.Runtime
{
    [Serializable]
    public class NotificationData
    {
        [FormerlySerializedAs("text"), TextArea] public string Text;
        public bool SetTextColor;
        [FormerlySerializedAs("textColor")] public Color TextColor;
        public bool SetBackgroundColor;
        [FormerlySerializedAs("backgroundColor")] public Color BackgroundColor;

        public NotificationData(string text)
        {
            Text = text;
        }

        public NotificationData(string text, Color textColor) : this(text)
        {
            SetTextColor = true;
            TextColor = textColor;
        }

        public NotificationData(string text, Color textColor, Color backgroundColor) : this(text, textColor)
        {
            SetBackgroundColor = true;
            BackgroundColor = backgroundColor;
        }
    }
}