using HHG.Common.Runtime;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace HHG.UISystem.Runtime
{
    [RequireComponent(typeof(Button))]
    public class UIButton : MonoBehaviour
    {
        [SerializeReference, SerializeReferenceDropdown] private List<IAction> actions = new List<IAction>();
        [SerializeReference, SerializeReferenceDropdown] private List<IActionAsync> actionsAsync = new List<IActionAsync>();

        private Button button;

        private void Awake()
        {
            button = GetComponent<Button>();

            button.onClick.AddListener(() => StartCoroutine(OnClick()));
        }

        private IEnumerator OnClick()
        {
            foreach (IAction action in actions)
            {
                action.DoAction(this);
            }

            foreach (IActionAsync action in actionsAsync)
            {
                yield return StartCoroutine(action.DoActionAsync(this));
            }
        }
    }
}
