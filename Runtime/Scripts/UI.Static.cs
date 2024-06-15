using HHG.Common.Runtime;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.UI;
using UnityEngine.SceneManagement;

namespace HHG.UISystem.Runtime
{
    public partial class UI
    {
        private static Dictionary<SubjectId, UI> map = new Dictionary<SubjectId, UI>();
        private static Stack<UI> opened = new Stack<UI>();
        private static InputAction back;

        public static event Action Back;

        static UI()
        {
            SceneManager.sceneUnloaded += OnSceneUnloaded;
            SceneManager.sceneLoaded += OnSceneLoaded;
        }

        private static void OnSceneUnloaded(Scene scene)
        {
            map.Clear(); // Shouldn't be needed but just in case
            opened.Clear(); // Since don't need to pop all before load new scene
        }

        private static void OnSceneLoaded(Scene scene, LoadSceneMode mode)
        {
            EventSystem current = EventSystem.current;

            if (current != null)
            {
                InputSystemUIInputModule module = current.currentInputModule as InputSystemUIInputModule ?? current.GetComponent<InputSystemUIInputModule>();

                if (module != null)
                {
                    if (module.cancel.action != back)
                    {
                        if (back != null)
                        {
                            back.performed -= OnBack;
                        }

                        back = module.cancel.action;
                        back.performed += OnBack;
                    }
                }
            }
        }

        private static void OnBack(InputAction.CallbackContext ctx)
        {
            if (Current && Current.backEnabled)
            {
                Pop();
                Back?.Invoke();
            }
        }

        public static UI Current => opened.Count > 0 ? opened.Peek() : null;
        public static int Count => opened.Count;
        public static void Refresh<T, TData>(object id, TData data) where T : UI<TData> => RefreshInternal(typeof(T), id, data);
        public static void Refresh<T, TData>(TData data) where T : UI<TData> => RefreshInternal(typeof(T), null, data);
        public static void Refresh(Type type, object id, object data) => RefreshInternal(type, id, data);
        public static void Refresh(Type type, object data) => RefreshInternal(type, null, data);
        public static Coroutine Open<T, TData>(object id, TData data, bool instant = false) where T : UI => OpenInternal(typeof(T), id, instant, data);
        public static Coroutine Open<T, TData>(TData data, bool instant = false) where T : UI => OpenInternal(typeof(T), null, instant, data);
        public static Coroutine Open<TData>(Type type, object id, TData data, bool instant = false) => OpenInternal(type, id, instant, data);
        public static Coroutine Open<TData>(Type type, TData data, bool instant = false) => OpenInternal(type, null, instant, data);
        public static Coroutine Open<T>(object id = null, bool instant = false) where T : UI => OpenInternal(typeof(T), id, instant);
        public static Coroutine Open<T>(bool instant) where T : UI => OpenInternal(typeof(T), null, instant);
        public static Coroutine Open(Type type, object id = null, bool instant = false) => OpenInternal(type, id, instant);
        public static Coroutine Open(Type type, bool instant) => OpenInternal(type, null, instant);
        public static Coroutine Close<T>(object id = null, bool instant = false) where T : UI => CloseInternal(typeof(T), id, instant);
        public static Coroutine Close<T>(bool instant) where T : UI => CloseInternal(typeof(T), null, instant);
        public static Coroutine Close(Type type, object id = null, bool instant = false) => CloseInternal(type, id, instant);
        public static Coroutine Close(Type type, bool instant) => CloseInternal(type, null, instant);
        public static Coroutine Focus<T>(object id = null, bool instant = false) where T : UI => FocusInternal(typeof(T), id, instant);
        public static Coroutine Focus<T>(bool instant) where T : UI => FocusInternal(typeof(T), null, instant);
        public static Coroutine Focus(Type type, object id = null, bool instant = false) => FocusInternal(type, id, instant);
        public static Coroutine Focus(Type type, bool instant) => FocusInternal(type, null, instant);
        public static Coroutine Unfocus<T>(object id = null, bool instant = false) where T : UI => UnfocusInternal(typeof(T), id, instant);
        public static Coroutine Unfocus<T>(bool instant) where T : UI => UnfocusInternal(typeof(T), null, instant);
        public static Coroutine Unfocus(Type type, object id = null, bool instant = false) => UnfocusInternal(type, id, instant);
        public static Coroutine Unfocus(Type type, bool instant) => UnfocusInternal(type, null, instant);
        public static Coroutine GoTo<T>(object id = null, bool instant = false) where T : UI => GoToInternal(typeof(T), id, instant);
        public static Coroutine GoTo<T>(bool instant) where T : UI => GoToInternal(typeof(T), null, instant);
        public static Coroutine GoTo(Type type, object id = null, bool instant = false) => GoToInternal(type, id, instant);
        public static Coroutine GoTo(Type type, bool instant) => GoToInternal(type, null, instant);
        public static Coroutine Push<T>(object id = null, bool instant = false) where T : UI => PushInternal(typeof(T), id, instant);
        public static Coroutine Push<T>(bool instant) where T : UI => PushInternal(typeof(T), null, instant);
        public static Coroutine Push(Type type, object id = null, bool instant = false) => PushInternal(type, id, instant);
        public static Coroutine Push(Type type, bool instant) => PushInternal(type, null, instant);
        public static Coroutine Pop(bool instant = false) => PopInternal(instant);
        public static Coroutine Pop(int amount, bool instant = false) => PopInternal(amount, instant);
        public static Coroutine Clear(bool instant = false) => ClearInternal(instant);
        public static Coroutine Swap<T>(object id = null, bool instant = false) where T : UI => SwapInternal(typeof(T), id, instant);
        public static Coroutine Swap<T>(bool instant) where T : UI => SwapInternal(typeof(T), null, instant);
        public static Coroutine Swap(Type type, object id = null, bool instant = false) => SwapInternal(type, id, instant);
        public static Coroutine Swap(Type type, bool instant) => SwapInternal(type, null, instant);

        private static void RefreshInternal(Type type, object id = null, object data = null)
        {
            SubjectId key = new SubjectId(type, id);

            if (map.TryGetValue(key, out UI ui) && ui is IRefreshableWeak refreshable)
            {
                refreshable.RefreshWeak(data);
            }
        }

        private static Coroutine OpenInternal(Type type, object id = null, bool instant = false, object data = null) => CoroutineUtil.StartCoroutine(OpenCoroutine(type, id, instant, data));
        private static Coroutine CloseInternal(Type type, object id = null, bool instant = false) => CoroutineUtil.StartCoroutine(CloseCoroutine(type, id, instant));
        private static Coroutine FocusInternal(Type type, object id = null, bool instant = false) => CoroutineUtil.StartCoroutine(FocusCoroutine(type, id, instant));
        private static Coroutine UnfocusInternal(Type type, object id = null, bool instant = false) => CoroutineUtil.StartCoroutine(UnfocusCoroutine(type, id, instant));
        private static Coroutine GoToInternal(Type type, object id = null, bool instant = false) => CoroutineUtil.StartCoroutine(GoToCoroutine(type, id, instant));
        private static Coroutine PushInternal(Type type, object id = null, bool instant = false) => CoroutineUtil.StartCoroutine(PushCoroutine(type, id, instant));
        private static Coroutine PopInternal(bool instant = false) => CoroutineUtil.StartCoroutine(PopCoroutine(instant));
        private static Coroutine PopInternal(int amount, bool instant = false) => CoroutineUtil.StartCoroutine(PopCoroutine(amount, instant));
        private static Coroutine ClearInternal(bool instant = false) => CoroutineUtil.StartCoroutine(ClearCoroutine(instant));
        private static Coroutine SwapInternal(Type type, object id = null, bool instant = false) => CoroutineUtil.StartCoroutine(SwapCoroutine(type, id, instant));

        private static IEnumerator OpenCoroutine(Type type, object id = null, bool instant = false, object data = null)
        {
            SubjectId key = new SubjectId(type, id);

            if (map.ContainsKey(key))
            {
                if (data != null && map[key] is IRefreshableWeak refreshable)
                {
                    refreshable.RefreshWeak(data);
                }
                yield return map[key].OpenInternal(instant);
            }
        }

        private static IEnumerator CloseCoroutine(Type type, object id = null, bool instant = false)
        {
            SubjectId key = new SubjectId(type, id);

            if (map.ContainsKey(key))
            {
                yield return map[key].CloseInternal(instant);
            }
        }

        private static IEnumerator FocusCoroutine(Type type, object id = null, bool instant = false)
        {
            SubjectId key = new SubjectId(type, id);

            if (map.ContainsKey(key))
            {
                yield return map[key].FocusInternal(instant);
            }
        }

        private static IEnumerator UnfocusCoroutine(Type type, object id = null, bool instant = false)
        {
            SubjectId key = new SubjectId(type, id);

            if (map.ContainsKey(key))
            {
                yield return map[key].UnfocusInternal(instant);
            }
        }

        // TODO: Need to not focus when popping deeper
        private static IEnumerator GoToCoroutine(Type type, object id = null, bool instant = false)
        {
            SubjectId key = new SubjectId(type, id);

            while (opened.Count > 0 && opened.Peek().SubjectId != key)
            {
                yield return PopInternal(instant);
            }

            if (opened.Count == 0)
            {
                yield return PushInternal(type, id, instant);
            }
        }

        private static IEnumerator PushCoroutine(Type type, object id = null, bool instant = false)
        {
            SubjectId key = new SubjectId(type, id);

            if (map.ContainsKey(key))
            {
                if (opened.Count > 0)
                {
                    yield return opened.Peek().UnfocusInternal(instant);
                }

                UI ui = map[key];
                opened.Push(ui);

                yield return ui.OpenInternal(instant);
                yield return ui.FocusInternal(instant);
            }
        }

        private static IEnumerator PopCoroutine(bool instant = false)
        {
            if (opened.Count > 0)
            {
                UI popped = opened.Pop();

                yield return popped.UnfocusInternal(instant);
                yield return popped.CloseInternal(instant);

                if (opened.Count > 0)
                {
                    yield return opened.Peek().FocusInternal(instant);
                }
            }
        }

        private static IEnumerator PopCoroutine(int amount, bool instant = false)
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

        private static IEnumerator ClearCoroutine(bool instant = false)
        {
            while (opened.Count > 0)
            {
                yield return PopInternal(instant);
            }
        }

        private static IEnumerator SwapCoroutine(Type type, object id = null, bool instant = false)
        {
            yield return PopInternal(instant);
            yield return PushInternal(type, id, instant);
        }
    }
}