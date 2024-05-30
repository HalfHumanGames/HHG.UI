using HHG.Common.Runtime;
using System;
using UnityEngine;

namespace HHG.UISystem.Runtime
{
    [Serializable]
    public class ModalButton
    {
        public string Text => text;
        public ActionEvent OnClick => onClick;

        [SerializeField] private string text;
        [SerializeField] private ActionEvent onClick = new ActionEvent();
    }
}