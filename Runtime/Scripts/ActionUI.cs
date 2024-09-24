using HHG.Common.Runtime;
using System;
using UnityEngine;
using UnityEngine.Serialization;

namespace HHG.UISystem.Runtime
{
    [Serializable]
    public class ActionUI : IAction
    {
        public enum Action
        {
            GoTo,
            Push,
            Pop,
            Clear,
            Swap,
            Open,
            Close,
            Focus,
            Unfocus
        }

        [SerializeField] private Action action;
        [SerializeField, Dropdown, FormerlySerializedAs("view")] private UIAsset ui;

        public void Invoke(MonoBehaviour invoker)
        {
            Type type = ui == null ? null : Type.GetType(ui.Value);

            if (ui is UIAssetT asset)
            {
                UI.Refresh(type, asset.WeakData);
            }

            switch (action)
            {
                case Action.GoTo:
                    UI.GoTo(type);
                    break;
                case Action.Push:
                    UI.Push(type);
                    break;
                case Action.Pop:
                    UI.Pop();
                    break;
                case Action.Clear:
                    UI.Clear();
                    break;
                case Action.Swap:
                    UI.Swap(type);
                    break;
                case Action.Open:
                    UI.Open(type);
                    break;
                case Action.Close:
                    UI.Close(type);
                    break;
                case Action.Focus:
                    UI.Focus(type);
                    break;
                case Action.Unfocus:
                    UI.Unfocus(type);
                    break;
            }
        }
    }
}