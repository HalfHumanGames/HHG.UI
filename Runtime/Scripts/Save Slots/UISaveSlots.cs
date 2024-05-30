using System.Collections.Generic;
using TMPro;
using UnityEngine;

namespace HHG.UISystem.Runtime
{
    public class UISaveSlots : UI<SaveSlots>
    {
        [SerializeField] private TextMeshProUGUI header;
        [SerializeField] private TextMeshProUGUI description;
        [SerializeField] private List<UIButton> buttons = new List<UIButton>();

        public override void Refresh(SaveSlots model)
        {
            header.text = model.Header;
            description.text = model.Description;

            //int count = Mathf.Min(buttons.Count, Modal.)
            //foreach (UIButton button in buttons)
            //{

            //}
        }
    }
}