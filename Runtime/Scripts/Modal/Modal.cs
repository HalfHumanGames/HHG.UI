using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Serialization;

namespace HHG.UISystem.Runtime
{
    [Serializable]
    public class Modal
    {
        [SerializeField, FormerlySerializedAs("header")] public string Header;
        [SerializeField, FormerlySerializedAs("description"), TextArea] public string Description;
        [SerializeField, FormerlySerializedAs("backEnabled")] public bool BackEnabled = true;
        [SerializeField, FormerlySerializedAs("buttons")] public List<ModalButton> Buttons = new List<ModalButton>();
    }
}