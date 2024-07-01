using HHG.Common.Runtime;
using System;
using UnityEngine;
using UnityEngine.Serialization;

namespace HHG.UISystem.Runtime
{
    [Serializable]
    public class ModalButton
    {
        [SerializeField, FormerlySerializedAs("text")] public string Text;
        [SerializeField, FormerlySerializedAs("onClick")] public ActionEvent OnClick = new ActionEvent();
    }
}