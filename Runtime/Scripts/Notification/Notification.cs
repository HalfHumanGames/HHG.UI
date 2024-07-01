using System;
using UnityEngine;
using UnityEngine.Serialization;

namespace HHG.UISystem.Runtime
{
    [Serializable]
    public class Notification
    {
        [SerializeField, FormerlySerializedAs("text"), TextArea] public string Text;
        [SerializeField, FormerlySerializedAs("textColor")] public Color TextColor;
        [SerializeField, FormerlySerializedAs("backgroundColor")] public Color BackgroundColor;
    }
}