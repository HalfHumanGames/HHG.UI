using HHG.Common;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace HHG.UI
{
    public partial class UI : MonoBehaviour
    {
        private static UI instance;

        public static UIView Current => instance.opened.Peek();
        public static Coroutine GoTo<T>(object id = null, bool instant = false) where T : UIView => instance.GoToInternal(typeof(T), id, instant);
        public static Coroutine GoTo(Type type, object id = null, bool instant = false) => instance.GoToInternal(type, id, instant);
        public static Coroutine Push<T>(object id = null, bool instant = false) where T : UIView => instance.PushInternal(typeof(T), id, instant);
        public static Coroutine Push(Type type, object id = null, bool instant = false) => instance.PushInternal(type, id, instant);
        public static Coroutine Pop(bool instant = false) => instance.PopInternal(instant);
        public static Coroutine Pop(int amount, bool instant = false) => instance.PopInternal(amount, instant);
        public static Coroutine Clear(bool instant = false) => instance.ClearInternal(instant);
        public static Coroutine Swap<T>(object id = null, bool instant = false) where T : UIView => instance.SwapInternal(typeof(T), id, instant);
        public static Coroutine Swap(Type type, object id = null, bool instant = false) => instance.SwapInternal(type, id, instant);

        private Dictionary<SubjectId, UIView> views = new Dictionary<SubjectId, UIView>();
        private Stack<UIView> opened = new Stack<UIView>();

        private void Awake()
        {
            instance = this;
            
            foreach (Transform child in transform)
            {
                UIView view = child.GetComponent<UIView>();
                views.Add(view.ViewId, view);
            }
        }

        private Coroutine GoToInternal(Type type, object id = null, bool instant = false) => StartCoroutine(GoToCoroutine(type, id, instant));
        private Coroutine PushInternal(Type type, object id = null, bool instant = false) => StartCoroutine(PushCoroutine(type, id, instant));
        private Coroutine PopInternal(bool instant = false) => StartCoroutine(PopCoroutine(instant));
        private Coroutine PopInternal(int amount, bool instant = false) => StartCoroutine(PopCoroutine(amount, instant));
        private Coroutine ClearInternal(bool instant = false) => StartCoroutine(ClearCoroutine(instant));
        private Coroutine SwapInternal(Type type, object id = null, bool instant = false) => StartCoroutine(SwapCoroutine(type, id, instant));

        private IEnumerator GoToCoroutine(Type type, object id = null, bool instant = false)
        {
            SubjectId key = new SubjectId(type, id);
            while (opened.Count > 0 && opened.Peek().ViewId != key)
            {
                yield return PopInternal(instant);
            }
            if (opened.Count == 0)
            {
                yield return PushInternal(type, id, instant);
            }
        }

        private IEnumerator PushCoroutine(Type type, object id = null, bool instant = false)
        {
            SubjectId key = new SubjectId(type, id);
            if (views.ContainsKey(key))
            {
                if (opened.Count > 0)
                {
                    yield return opened.Peek().Unfocus(instant);
                }

                UIView view = views[key];
                opened.Push(view);
                yield return view.Open(instant);
                yield return view.Focus(instant);
            }
        }

        private IEnumerator PopCoroutine(bool instant = false)
        {
            if (opened.Count > 0)
            {
                UIView popped = opened.Pop();
                yield return popped.Unfocus(instant);
                yield return popped.Close(instant);

                if (opened.Count > 0)
                {
                    yield return opened.Peek().Focus(instant);
                }
            }
        }

        private IEnumerator PopCoroutine(int amount, bool instant = false)
        {
            if (amount > opened.Count)
            {
                amount = opened.Count;
            }
            for (int i = 0; i < amount; i++)
            {
                yield return PopInternal(instant);
            }
        }

        private IEnumerator ClearCoroutine(bool instant = false)
        {
            while (opened.Count > 0)
            {
                yield return PopInternal(instant);
            }
        }

        private IEnumerator SwapCoroutine(Type type, object id = null, bool instant = false)
        {
            yield return PopInternal(instant);
            yield return PushInternal(type, id, instant);
        }
    }
}
