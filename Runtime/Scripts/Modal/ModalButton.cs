using HHG.Common.Runtime;
using System;
using System.Collections.Generic;
using UnityEngine;

namespace HHG.UISystem.Runtime
{
    [Serializable]
    public class ModalButton
    {
        public string Text => text;
        public List<IAction> Actions => actions;
        public List<IActionAsync> ActionsAsync => actionsAsync;

        [SerializeField] private string text;
        [SerializeReference, SerializeReferenceDropdown] private List<IAction> actions = new List<IAction>();
        [SerializeReference, SerializeReferenceDropdown] private List<IActionAsync> actionsAsync = new List<IActionAsync>();
    }
}