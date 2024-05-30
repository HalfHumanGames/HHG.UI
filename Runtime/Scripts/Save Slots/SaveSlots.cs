using HHG.Common.Runtime;
using System;
using UnityEngine;

namespace HHG.UISystem.Runtime
{
    [Serializable]
    public class SaveSlots
    {
        public string Header => header;
        public string Description => description;
        public ActionEvent OnClickExists => onClickExists;
        public ActionEvent OnClickEmpty => onClickEmpty;

        [SerializeField] private string header;
        [SerializeField] private string description;
        [SerializeReference, SerializeReferenceDropdown] private ActionEvent onClickExists = new ActionEvent();
        [SerializeReference, SerializeReferenceDropdown] private ActionEvent onClickEmpty = new ActionEvent();
    }
}