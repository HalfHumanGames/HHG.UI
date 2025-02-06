using HHG.Common.Runtime;
using System;
using UnityEngine.Serialization;

namespace HHG.UISystem.Runtime
{
    [Serializable]
    public class ModalButton
    {
        [FormerlySerializedAs("text")] public string Text;
        [FormerlySerializedAs("onClick")] public ActionEvent OnClick = new ActionEvent();
    }
}