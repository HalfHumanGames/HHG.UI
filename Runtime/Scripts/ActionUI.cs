using HHG.Common.Runtime;
using System;
using UnityEditor;
using UnityEngine;

namespace HHG.UISystem.Runtime
{
    public class ActionUI : IAction
    {
        public enum Action
        {
            GoTo,
            Push,
            Pop,
            Clear,
            Swap
        }

        [SerializeField] private Action action;
        [SerializeField] private ViewNameAsset view;

        public void DoAction(MonoBehaviour invoker)
        {
            Type type = Type.GetType(view);
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
            }
        }
    }
}