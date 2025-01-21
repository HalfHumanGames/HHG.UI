using System;
using UnityEngine;
using UnityEngine.Serialization;

namespace HHG.UISystem.Runtime
{
    [Serializable]
    public class Notification
    {
        [FormerlySerializedAs("text"), TextArea] public string Text;
        public bool SetTextColor;
        [FormerlySerializedAs("textColor")] public Color TextColor;
        public bool SetBackgroundColor;
        [FormerlySerializedAs("backgroundColor")] public Color BackgroundColor;
    }
}