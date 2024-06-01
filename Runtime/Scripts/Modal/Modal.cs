using System;
using UnityEngine;

namespace HHG.UISystem.Runtime
{
    [Serializable]
    public class Modal
    {
        public string Header => header;
        public string Description => description;
        public ModalButton[] Buttons => buttons;

        [SerializeField] private string header;
        [SerializeField, TextArea] private string description;
        [SerializeField] private ModalButton[] buttons;
    }
}