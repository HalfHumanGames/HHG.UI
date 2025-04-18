using HHG.Common.Runtime;
using System.Linq;
using TMPro;
using UnityEngine;

namespace HHG.UI.Runtime
{
    public class UIInputFieldModal : UIModal<InputFieldModalData>
    {
        public string InputFieldText => inputField.text;

        [SerializeField] private TMP_InputField inputField;

        public override void Refresh(InputFieldModalData data)
        {
            base.Refresh(data);

            inputField.text = data.InputFieldText;

            inputField.SetNavigationDown(buttons.FirstOrDefault().Button);
            buttons.Select(b => b.Button).SetNavigationUp(inputField);
        }
    }
}