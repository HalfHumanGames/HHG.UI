using System;
using UnityEngine;

namespace HHG.UISystem.Runtime
{
    [Serializable]
    public class Notification
    {
        public string Text => text;
        public Color TextColor => textColor;
        public Color BackgroundColor => backgroundColor;

        [SerializeField, TextArea] private string text;
        [SerializeField] private Color textColor;
        [SerializeField] private Color backgroundColor;
    }
}