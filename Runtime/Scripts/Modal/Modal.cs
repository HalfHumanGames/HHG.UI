using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Serialization;

namespace HHG.UISystem.Runtime
{
    [Serializable]
    public class Modal
    {
        [FormerlySerializedAs("header")] public string Header;
        [FormerlySerializedAs("description"), TextArea] public string Description;
        [FormerlySerializedAs("backEnabled")] public bool BackEnabled = true;
        [FormerlySerializedAs("buttons")] public List<ModalButton> Buttons = new List<ModalButton>();
    }
}